/*
 * Replace the following string of 0s with your student number
 * 200972727
 */
#include <stdio.h>
#include <unistd.h>
#include <sys/stat.h>
#include <errno.h>
#include "joblog.h"

/* 
 * DO NOT EDIT the new_log_name function. It is a private helper 
 * function provided for you to create a log name from a process 
 * descriptor for use when reading, writing and deleting a log file.
 * 
 * You must work out what the function does in order to use it properly
 * and to clean up after use.
 */
static char* new_log_name(proc_t* proc) {
    static char* joblog_name_fmt = "%s/%.31s%07d.txt";
                                // string format for the name of a log file
                                // declared static to have only one instance

    if (!proc)
        return NULL;

    char* log_name;
            
    asprintf(&log_name, joblog_name_fmt, JOBLOG_PATH, proc->type_label,
        proc->id);

    return log_name;
}

/* 
 * DO NOT EDIT the joblog_init function that sets up the log directory 
 * if it does not already exist.
 */
int joblog_init(proc_t* proc) {
    if (!proc) {
        errno = EINVAL;
        return -1;
    }
        
    int r = 0;
    if (proc->is_init) {
        struct stat sb;
    
        if (stat(JOBLOG_PATH, &sb) != 0) {
            errno = 0;
            r = mkdir(JOBLOG_PATH, 0777);
        }  else if (!S_ISDIR(sb.st_mode)) {
            unlink(JOBLOG_PATH);
            errno = 0;
            r = mkdir(JOBLOG_PATH, 0777);
        }
    }

    joblog_delete(proc);    // in case log exists for proc
    
    return r;
}

/* 
 * TODO: you must implement this function.
 * Hints:
 * - you have to go to the beginning of the line represented
 *      by entry_num to read the required entry
 * - see job.h for a function to create a job from its string representation
 */
job_t* joblog_read(proc_t* proc, int entry_num, job_t* job) {
	
	int i = errno;
	char* name = new_log_name(proc);
	if(name != NULL && entry_num>=0){
	
	FILE* log = fopen(name, "r");
	if(log == NULL && errno == ENOENT){
		errno = i;
		return NULL;
	}
		
	char row[JOB_STR_SIZE];
	fseek(log, entry_num * JOB_STR_SIZE, SEEK_SET);
	(void) fgets(row, JOB_STR_SIZE, log);
	
	if(row == NULL){
		fclose(log);
		return NULL;
	}
	if(job == NULL){job = malloc(sizeof(job_t));}
	fclose(log);
	job = str_to_job(row, job);
	return job;
	}
    return NULL;
}

/* 
 * TODO: you must implement this function.
 * Hints:
 * - remember new entries are appended to a log file
 * - see the hint for joblog_read
 */
void joblog_write(proc_t* proc, job_t* job) {
	int i = errno;
	char* name = new_log_name(proc);
	if(name != NULL && job != NULL){
		FILE* log = fopen(name, "a");
		char row[JOB_STR_SIZE];
		fprintf(log, "%s\n", job_to_str(job, row));
		if(errno != i){
			errno = i;
			fclose(log);
			return;
		}
		fclose(log);
	}
    return;
}

/* 
 * TODO: you must implement this function.
 */
void joblog_delete(proc_t* proc) {
	if (proc){
	char* name = new_log_name(proc);
	(void) remove(name);
	}
    return;
}