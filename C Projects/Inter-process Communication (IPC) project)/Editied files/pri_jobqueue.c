/*
 * Replace the following string of 0s with your student number
 * 200972727
 */
#include <stdlib.h>
#include <stdbool.h>
#include "pri_jobqueue.h"

/* 
 * TODO: you must implement this function that allocates a job queue and 
 * initialise it.
 * Hint:
 * - see job_new in job.c
 */
pri_jobqueue_t* pri_jobqueue_new() {
	pri_jobqueue_t *pjq = (pri_jobqueue_t*) malloc(sizeof(pri_jobqueue_t));
	pri_jobqueue_init(pjq);
	return pjq;
}

/* 
 * TODO: you must implement this function.
 */
void pri_jobqueue_init(pri_jobqueue_t* pjq) {
	pjq -> buf_size = JOB_BUFFER_SIZE;
	pjq -> size = 0;
	for(int i=0; i < JOB_BUFFER_SIZE; i++){
	job_init(&pjq -> jobs[i]);
	}
    return;
}

/* 
 * TODO: you must implement this function.
 * Hint:
 *      - if a queue is not empty, and the highest priority job is not in the 
 *      last used slot on the queue, dequeueing a job will result in the 
 *      jobs on the queue having to be re-arranged
 *      - remember that the job returned by this function is a copy of the job
 *      that was on the queue
 */
job_t* pri_jobqueue_dequeue(pri_jobqueue_t* pjq, job_t* dst) {
	if(!pri_jobqueue_is_empty(pjq)){
		int size = pri_jobqueue_size(pjq);
		int current_prio = (&pjq -> jobs[0]) -> priority;
		int current_index = 0;
		for(int i = 1; i< size; i++){
			if(current_prio > (&pjq -> jobs[i]) -> priority){
				current_index = i;
				current_prio = (&pjq -> jobs[i]) -> priority;
			}
		}
		dst = job_copy(&pjq -> jobs[current_index], dst);
		for(int i = current_index; i< size - 1; i++){
			job_copy(&pjq -> jobs[i + 1], &pjq -> jobs[i]);
		}
		job_init(&pjq -> jobs[size-1]);
		pjq -> size--;
		return dst;
	}
    else{return NULL;}
}

/* 
 * TODO: you must implement this function.
 * Hints:
 * - if a queue is not full, and if you decide to store the jobs in 
 *      priority order on the queue, enqueuing a job will result in the jobs 
 *      on the queue having to be re-arranged. However, it is not essential to
 *      store jobs in priority order (it simplifies implementation of dequeue
 *      at the expense of extra work in enqueue). It is your choice how 
 *      you implement dequeue (and enqueue) to ensure that jobs are dequeued
 *      by highest priority job first (see pri_jobqueue.h)
 * - remember that the job passed to this function is copied to the 
 *      queue
 */
void pri_jobqueue_enqueue(pri_jobqueue_t* pjq, job_t* job) {
	if(pjq != NULL && job != NULL && !pri_jobqueue_is_full(pjq) && job->priority>0){
		int size = pri_jobqueue_size(pjq);
		job_copy(job, &pjq -> jobs[size]);
		pjq -> size++;
	}
    return;
}
   
/* 
 * TODO: you must implement this function.
 */
bool pri_jobqueue_is_empty(pri_jobqueue_t* pjq) {
	if(pri_jobqueue_size(pjq) != 0){return false;}
    else{return true;}
}

/* 
 * TODO: you must implement this function.
 */
bool pri_jobqueue_is_full(pri_jobqueue_t* pjq) {
	if(pri_jobqueue_space(pjq) != 0){return false;}
    else{return true;}
}

/* 
 * TODO: you must implement this function.
 * Hints:
 *      - remember that the job returned by this function is a copy of the 
 *      highest priority job on the queue.
 *      - both pri_jobqueue_peek and pri_jobqueue_dequeue require copying of 
 *      the highest priority job on the queue
 */
job_t* pri_jobqueue_peek(pri_jobqueue_t* pjq, job_t* dst) {
	if(!pri_jobqueue_is_empty(pjq)){
		int size = pri_jobqueue_size(pjq);
		int current_prio = (&pjq -> jobs[0]) -> priority;
		int current_index = 0;
		for(int i = 1; i< size; i++){
			if(current_prio > (&pjq -> jobs[i]) -> priority){
				current_index = i;
				current_prio = (&pjq -> jobs[i]) -> priority;
			}
		}
		dst = job_copy(&pjq -> jobs[current_index], dst);
		return dst;
	}
    return NULL;
}

/* 
 * TODO: you must implement this function.
 */
int pri_jobqueue_size(pri_jobqueue_t* pjq) {
	int size = 0;
    if(pjq != NULL){
		size = pjq -> size;
		}
	return size;
}

/* 
 * TODO: you must implement this function.
 */
int pri_jobqueue_space(pri_jobqueue_t* pjq) {
	if(pjq != NULL){return pjq -> buf_size - pri_jobqueue_size(pjq);}
	else{return 0;}
}

/* 
 * TODO: you must implement this function.
 *  Hint:
 *      - see pri_jobqeue_new
 */
void pri_jobqueue_delete(pri_jobqueue_t* pjq) {
	if(pjq){
		free(pjq);
	}
    return;
}
