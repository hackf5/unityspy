namespace HackF5.UnitySpy.ProcessFacade
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using System.Text;
    using HackF5.UnitySpy.Util;
    using JetBrains.Annotations;

    /// <summary>
    /// A Windows specific facade over a process that provides access to its memory space.
    /// </summary>
    [PublicAPI]
    public class ProcessFacadeClient
    {
        public const int Port = 39185;
        public const int BufferSize = 4096;
        public const int RequestSize = 17;
        public const byte ReadMemoryRequestType = 0;
        public const byte GetModuleRequestType = 1;

        private int processId;

        private byte[] internalBuffer = new byte[BufferSize];

        private Socket socket;

        public ProcessFacadeClient(int processId)
        {
            this.processId = processId;
        }

        public void ReadProcessMemory(
            byte[] buffer,
            IntPtr processAddress,
            int length)
        {
            lock (this)
            {
                int bufferIndex = 0;
                Request request = new Request(ReadMemoryRequestType, this.processId, processAddress, length);
                
                // For debbuging
                // Console.WriteLine($"Requested address = {processAddress.ToString("X")}, length = {length}");  
                    
                int bytesRec = 0;
                try
                {
                    if (this.socket == null)
                    {
                        this.Connect();
                    }

                    // Send the data through the socket.
                    int bytesSent = this.socket.Send(request.GetBytes());

                    // Receive the response from the remote device.
                    do
                    {
                        bytesRec = this.socket.Receive(this.internalBuffer);
                        Array.Copy(this.internalBuffer, 0, buffer, bufferIndex, bytesRec);
                        bufferIndex += bytesRec;
                        length -= bytesRec;
                    }
                    while (length > 0);
                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                    this.CloseConnection();
                }
                catch (ArgumentException ae)
                {
                    Console.WriteLine($"address = {processAddress.ToString("X")}, length = {length}, Bytes Rec = {bytesRec}, bufferIndex = {bufferIndex}, buffer length = {buffer.Length}");  
                    Console.WriteLine("ArgumentException : {0}", ae.ToString());                
                    this.CloseConnection();
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                    this.CloseConnection();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                    this.CloseConnection();
                }
            }
        }

        public ModuleInfo GetModuleInfo(string moduleName)
        {
            lock (this)
            {
                byte[] moduleNameInAscii = Encoding.ASCII.GetBytes(moduleName);
                Request request = new Request(GetModuleRequestType, this.processId, IntPtr.Zero, moduleNameInAscii.Length);
                try
                {
                    if (this.socket == null)
                    {
                        this.Connect();
                    }

                    // Send the data through the socket.
                    int bytesSent = this.socket.Send(request.GetBytes());
                    bytesSent = this.socket.Send(moduleNameInAscii);

                    // Receive the base address
                    this.socket.Receive(this.internalBuffer, 8, SocketFlags.None);
                    IntPtr baseAddress = (IntPtr)this.internalBuffer.ToUInt64();

                    // Receive the size
                    this.socket.Receive(this.internalBuffer, 8, SocketFlags.None);
                    uint size = this.internalBuffer.ToUInt32();

                    string path = this.GetString();
                    return new ModuleInfo(moduleName, baseAddress, size, path);
                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                    this.CloseConnection();
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                    this.CloseConnection();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                    this.CloseConnection();
                }
                
                throw new Exception($"Could not find the {moduleName} module");
            }
        }

        private string GetString()
        {
            // Receive the length of the string
            int bytesRec = this.socket.Receive(this.internalBuffer, 4, SocketFlags.None);

            int length = this.internalBuffer.ToInt32();

            byte[] buffer = new byte[length];
            int bufferIndex = 0;
            do
            {
                bytesRec = this.socket.Receive(this.internalBuffer);

                Array.Copy(this.internalBuffer, 0, buffer, bufferIndex, bytesRec);
                bufferIndex += bytesRec;
                length -= bytesRec;
            }
            while (length > 0);
            return buffer.ToAsciiString();
        }

        // Connect to the server
        private void Connect()
        {
            Console.WriteLine("Connecting to server...");

            // Establish the remote endpoint for the socket.
            IPHostEntry localhost = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = localhost.AddressList[0];
            IPEndPoint serverEndPoint = new IPEndPoint(ipAddress, Port);

            // Create a TCP/IP  socket.
            this.socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            this.socket.Connect(serverEndPoint);
        }

        private void CloseConnection()
        {
            if (this.socket != null)
            {
                Console.WriteLine("Disconnecting from server...");
                try
                {
                    // Release the socket
                    this.socket.Shutdown(SocketShutdown.Both);
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }
                finally
                {
                    this.socket.Close();
                    this.socket = null;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Request
        {
            private byte type;
            private int pid;
            private IntPtr address;
            private int size;

            public Request(byte type, int pid, IntPtr address, int size)
            {
                this.type = type;
                this.pid = pid;
                this.address = address;
                this.size = size;
            }

            public byte[] GetBytes()
            {
                byte[] arr = new byte[RequestSize];
                arr[0] = type;

                IntPtr ptr = Marshal.AllocHGlobal(RequestSize);
                Marshal.StructureToPtr(this, ptr, true);

                // start at 4 because the byte type in C# is just an int
                Marshal.Copy(ptr + 4, arr, 1, RequestSize - 1);
                Marshal.FreeHGlobal(ptr);
                return arr;
            }

            public override string ToString()
            {
                return $"Address = {this.address.ToString("X")}, size = {this.size}";
            }
        }
    }
}