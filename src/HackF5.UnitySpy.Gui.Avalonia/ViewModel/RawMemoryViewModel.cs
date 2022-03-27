namespace HackF5.UnitySpy.Gui.Avalonia.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Reactive;
    using System.Text;
    using System.Threading.Tasks;
    using global::Avalonia.Controls;
    using HackF5.UnitySpy.ProcessFacade;
    using HackF5.UnitySpy.Util;
    using ReactiveUI;

    public class RawMemoryViewModel : ReactiveObject
    {        
        private ProcessFacade process;

        private string bufferSize = "1024";

        private string startAddress;

        private byte[] buffer;

        public RawMemoryViewModel()
        {
            this.Refresh = ReactiveCommand.Create(this.StartRefresh);
            this.StartRefresh();
        }     

        public ProcessFacade Process
        {
            set {           
                if (this.process != value)     
                {
                    this.process = value;
                    this.StartRefresh();
                }
            }
        }  
                                
        public string BufferSize
        {
            get => this.bufferSize;
            set {
                if(this.bufferSize != value)
                {
                    this.RaiseAndSetIfChanged(ref this.bufferSize, value);    
                    this.InitBuffer();             
                    this.StartRefresh();
                }
            }
        }      
                                
        public string StartAddress
        {
            get => this.startAddress;
            set {
                if(this.startAddress != value)
                {
                    this.RaiseAndSetIfChanged(ref this.startAddress, value);                 
                    this.StartRefresh();
                }
            }
        }

        public ObservableCollection<string> BufferLines { get; } =
            new ObservableCollection<string>();

        public ReactiveCommand<Unit, Unit> Refresh { get; }

        private void InitBuffer()
        {
            this.buffer = new byte[int.Parse(this.bufferSize)];
        }

        private void StartRefresh() 
        {
            Task.Run(() => this.ExecuteRefresh());
        }

        private void ExecuteRefresh()
        {
            if (this.process == null)
            {
                return;
            }

            if (this.buffer == null)
            {
                this.InitBuffer();
            }
            IntPtr startAddressPtr = new IntPtr(Convert.ToInt64(this.startAddress, 16));
            this.process.ReadProcessMemory(this.buffer, startAddressPtr, true, this.buffer.Length);

            this.BufferLines.Clear();

            int bytesPerLine = 32;            
            for (int i = 0; i < this.buffer.Length; i += bytesPerLine)
            {
                IntPtr address = startAddressPtr + i;
                StringBuilder line = new StringBuilder(address.ToString("X"));
                line.Append(": ");
                StringBuilder charLine = new StringBuilder();
                for (int j = 0; j < bytesPerLine; j++)
                {
                    line.Append(buffer[i + j].ToString("X2"));
                    line.Append(' ');
                    charLine.Append(ToAsciiSymbol(this.buffer[i + j]));
                }
                line.Append(charLine);
                this.BufferLines.Add(line.ToString());
            }
        }  

        private static char ToAsciiSymbol(byte value)
        {
            if (value < 32) return '.';  // Non-printable ASCII
            if (value < 127) return (char)value;   // Normal ASCII
            // Handle the hole in Latin-1
            if (value == 127) return '.';
            if (value < 0x90) return "€.‚ƒ„…†‡ˆ‰Š‹Œ.Ž."[ value & 0xF ];
            if (value < 0xA0) return ".‘’“”•–—˜™š›œ.žŸ"[ value & 0xF ];
            if (value == 0xAD) return '.';   // Soft hyphen: this symbol is zero-width even in monospace fonts
            return (char) value;   // Normal Latin-1
        }
    }
}
