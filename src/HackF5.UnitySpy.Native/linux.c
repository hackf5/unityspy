#include <stdio.h>
#include <dirent.h>
#include <stdint.h>

#include "server.h"

int32_t read_process_memory_to_buffer(int pid, uint64_t addr, char* buffer, uint32_t size)
{
    char filepath[50];
    int32_t bytes_to_read = 0;

    //Generates the path to the file to be read
    sprintf(filepath, "/proc/%i/mem", pid);

    FILE *fp = fopen(filepath, "r");
    if(fp == NULL)
    {
        perror("Open file error");
        printf("File: %s, pid as i: %i, pid as u: %u\n", filepath, pid, pid);  
    }
    else
    {        
        fseek(fp, addr, SEEK_SET);
        bytes_to_read = fread(buffer, sizeof(char), size, fp);
        fclose(fp);
    }  
    
    return bytes_to_read;    
}

char* get_module_info(int pid, const char* module_name, int* module_size, char* path)
{
    return NULL;
}