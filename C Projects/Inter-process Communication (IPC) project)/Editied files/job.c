/*
 * Replace the following string of 0s with your student number
 * 200972727
 */
#include <stdlib.h>
#include <string.h>
#include <stdio.h>
#include "job.h"

/* 
 * DO NOT EDIT the job_new function.
 */
job_t* job_new(pid_t pid, unsigned int id, unsigned int priority, 
    const char* label) {
    return job_set((job_t*) malloc(sizeof(job_t)), pid, id, priority, label);
}

/* 
 * TODO: you must implement this function
 */
job_t* job_copy(job_t* src, job_t* dst) {
	if (src != NULL && src != dst){
		if (dst == NULL){
			return job_new(src -> pid, src -> id, src -> priority, src -> label);
		}
		else{
			dst -> pid = src -> pid;
			dst -> id = src -> id;
			dst -> priority = src -> priority;
			snprintf(dst -> label, MAX_NAME_SIZE, "%s", src -> label);
			return dst;
		}
		}
    return src;
}

/* 
 * TODO: you must implement this function
 */
void job_init(job_t* job) {
	if(job != NULL){
		job -> pid = 0;
		job -> id = 0;
		job -> priority = 0;
		snprintf(job -> label, MAX_NAME_SIZE, "%s", PAD_STRING);
	}
    return;
}

/* 
 * TODO: you must implement this function
 */
bool job_is_equal(job_t* j1, job_t* j2) {
	if(j1 == NULL || j2 == NULL){
		if(j1 == NULL && j2 ==NULL){
			return true;
			}
		else{return false;}
	}
	else {
		if(j1 -> pid == j2 -> pid && j1 -> id == j2 -> id &&
		j1 -> priority == j2 -> priority && strncmp(j1 -> label, j2 -> label, MAX_NAME_SIZE - 1) == 0){
			return true;
	}
    return false;
	}
}

/*
 * TODO: you must implement this function.
 * Hint:
 * - read the information in job.h about padding and truncation of job
 *      labels
 */
job_t* job_set(job_t* job, pid_t pid, unsigned int id, unsigned int priority,
    const char* label) {
	if (job != NULL){
	job -> pid = pid;
	job -> id = id;
	job -> priority = priority;
	if (label == NULL){label = "";}
	snprintf(job -> label, MAX_NAME_SIZE, "%s%s", label, PAD_STRING);
	}
    return job;
}

/*
 * TODO: you must implement this function.
 * Hint:
 * - see malloc and calloc system library functions for dynamic allocation, 
 *      and the documentation in job.h for when to do dynamic allocation
 */
char* job_to_str(job_t* job, char* str) {
	if(job!= NULL && strnlen(job -> label, MAX_NAME_SIZE) == MAX_NAME_SIZE - 1){
	if(str != NULL){
	snprintf(str, JOB_STR_SIZE, JOB_STR_FMT, job -> pid, job -> id, job -> priority, job -> label);
	return str;
	}
	else{
		char* buffer = NULL;
		buffer = (char*)calloc(JOB_STR_SIZE, sizeof(int) *sizeof(int) *sizeof(int) *MAX_NAME_SIZE);
		snprintf(buffer, JOB_STR_SIZE, JOB_STR_FMT, job -> pid, job -> id, job -> priority, job -> label);
		return buffer;
	}
	}
	
    return NULL;
}

/*
 * TODO: you must implement this function.
 * Hint:
 * - see the hint for job_to_str
 */
job_t* str_to_job(char* str, job_t* job) {
	
	if(str == NULL || strnlen(str, JOB_STR_SIZE) != JOB_STR_SIZE - 1){return NULL;}
	
	int pid, id, priority;
	char* label;
	label = (char*) malloc(MAX_NAME_SIZE);
	int i = sscanf(str, JOB_STR_FMT, &pid, &id, &priority, label);
	
	if(i == 4 && strnlen(label, MAX_NAME_SIZE) == MAX_NAME_SIZE - 1){ //if format wrong i != 4
	if(job != NULL){
	job -> pid = pid;
	job -> id = id;
	job -> priority = priority;
	snprintf(job -> label, MAX_NAME_SIZE, "%s", label);
	free(label);
	return job;
	}
	else{
		job_t* j = job_new(pid, id, priority, label);
		free(label);
		return j;
		}
	}
	free(label);
    return NULL;
}

/* 
 * TODO: you must implement this function
 * Hint:
 * - look at the allocation of a job in job_new
 */
void job_delete(job_t* job) {
	if(job){
		free(job);
	}
    return;
}
