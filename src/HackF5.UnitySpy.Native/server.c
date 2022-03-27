#include <stdio.h>
#include <stdint.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <arpa/inet.h>	// inet_addr
#include <unistd.h>	// write
#include <pthread.h> // threading, link with lpthread

#include "server.h"

#if defined(__linux__)
#include "linux.c"
#elif defined(__MACH__)
#include "macos.c"
#endif

#define PORT      39185

#define READ_MEMORY_REQUEST 0
#define GET_MODULE_REQUEST 1
#define REQUEST_SIZE 17 // byte 0 = request type, byte 1-5 = pid, byte 5-13 = address, byte 14-17 = size

/*
 * This will handle connection for each client
 * */
void *connection_handler(void* socket_ptr)
{    
    int socket = *(int*)socket_ptr;
    char buff[BUFFER_SIZE]; 
    char request[REQUEST_SIZE]; 
    int32_t* pid = request + 1;
    int64_t* address = request + 5;
    int32_t* size = request + 13;

    //Receive a request from the client
    while(recv(socket, request, REQUEST_SIZE, 0) == REQUEST_SIZE)
    {
        if (request[0] == GET_MODULE_REQUEST)
        {   
            puts("Requested Module");  
            if (recv(socket, buff, *size, 0) < 0)
            {
                puts("recv module name failed");
                break;
            }
            else
            {
                buff[(int)size] = '\0';
                int module_size;
                int64_t baseAddress = get_module_info(*pid, buff, &module_size, buff);

                if(send(socket, &baseAddress, sizeof(baseAddress), 0) < 0)
                {
                    perror("Fail to send address module info");
                    break;
                }

                if(send(socket, &module_size, sizeof(module_size), 0) < 0)
                {
                    perror("Fail to send size module info");
                    break;
                }

                if(send(socket, buff, strlen(buff), 0) < 0)
                {
                    perror("Fail to send path module info");
                    break;
                }
            }
        } 
        else if (request[0] == READ_MEMORY_REQUEST)
        {                                  
            uint32_t bytes_to_read;
            uint32_t remaining_bytes = *size;
            uint64_t address_limit = *address + remaining_bytes;

            // printf("Memory chunk requested: pid = %i, address = %08X, size = %u\n", *pid, *address, remaining_bytes);  

            while (remaining_bytes > 0)
            {                 
                if(remaining_bytes < BUFFER_SIZE)
                {
                    bytes_to_read = remaining_bytes;
                } 
                else
                {
                    bytes_to_read = BUFFER_SIZE;
                }
                bytes_to_read = read_process_memory_to_buffer(*pid, address_limit - remaining_bytes, buff, bytes_to_read);
                remaining_bytes -= bytes_to_read;
                if(send(socket, buff, bytes_to_read, 0) != bytes_to_read)
                {
                    perror("Fail to send file");
                    break;
                }
            }
        }      
        else
        {
            printf("Request not recognized %u\n", request[0]);  
        }
    }

    puts("Closing connection");
    close(socket);
    fflush(stdout);
} 

int main(int argc, char *argv[])
{
	int socket_desc, new_socket, c;
	struct sockaddr_in server, client;
	pthread_t thread_id;
	
	//Create socket
	socket_desc = socket(AF_INET, SOCK_STREAM, 0);
	if (socket_desc == -1)
	{
		printf("Could not create socket");
	}
	
	//Prepare the sockaddr_in structure
	server.sin_family = AF_INET;
	server.sin_addr.s_addr = INADDR_ANY;
	server.sin_port = htons( PORT );
	
	//Bind
	if( bind(socket_desc,(struct sockaddr *)&server , sizeof(server)) < 0)
	{
		puts("bind failed");
		return 1;
	}
	puts("bind done");
	
	//Listen
	listen(socket_desc, 3);
	
	//Accept and incoming connection
	puts("Waiting for incoming connections...");
	c = sizeof(struct sockaddr_in);
	while( (new_socket = accept(socket_desc, (struct sockaddr *)&client, (socklen_t*)&c)) )
	{		
        if( pthread_create( &thread_id , NULL ,  connection_handler , (void*) &new_socket) < 0)
        {
            perror("could not create thread");
            return 1;
        }
	}
	
    puts("Exiting...");
    fflush(stdout);
    
    if (new_socket < 0)
	{
		perror("accept failed");
		return 1;
	}
	
	return 0;
}