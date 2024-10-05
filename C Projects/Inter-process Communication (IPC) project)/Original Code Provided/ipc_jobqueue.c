/*
 * Replace the following string of 0s with your student number
 * 000000000
 */
#include "ipc_jobqueue.h"

/* 
 * DO NOT EDIT the ipc_jobqueue_new function.
 */
ipc_jobqueue_t* ipc_jobqueue_new(proc_t* proc) {
    ipc_jobqueue_t* ijq = ipc_new(proc, "ipc_jobq", sizeof(pri_jobqueue_t));
    
    if (!ijq) 
        return NULL;
    
    if (proc->is_init)
        pri_jobqueue_init((pri_jobqueue_t*) ijq->addr);
    
    return ijq;
}

/* 
 * TODO: you must implement this function.
 * Hints:
 * - this is a wrapper for jobqueue function jobqueue_dequeue
 * - and remember you must call do_critical_work
 */
job_t* ipc_jobqueue_dequeue(ipc_jobqueue_t* ijq, job_t* dst) {
    return NULL;
}

/* 
 * TODO: you must implement this function.
 * Hint:
 * - see ipc_jobqueue_dequeue hint
 */
void ipc_jobqueue_enqueue(ipc_jobqueue_t* ijq, job_t* job) {
    return;
}
    
/* 
 * TODO: you must implement this function.
 * Hint:
 * - see ipc_jobqueue_dequeue hint
 */
bool ipc_jobqueue_is_empty(ipc_jobqueue_t* ijq) {
    return true;
}

/* 
 * TODO: you must implement this function.
 * Hint:
 * - see ipc_jobqueue_dequeue hint
 */
bool ipc_jobqueue_is_full(ipc_jobqueue_t* ijq) {
    return true;
}

/* 
 * TODO: you must implement this function.
 * Hint:
 * - see ipc_jobqueue_dequeue hint
 */
job_t* ipc_jobqueue_peek(ipc_jobqueue_t* ijq, job_t* dst) {
    return NULL;
}

/* 
 * TODO: you must implement this function.
 * Hint:
 * - see ipc_jobqueue_dequeue hint
 */
int ipc_jobqueue_size(ipc_jobqueue_t* ijq) {
    return 0;
}

/* 
 * TODO: you must implement this function.
 * Hint:
 * - see ipc_jobqueue_dequeue hint
 */
int ipc_jobqueue_space(ipc_jobqueue_t* ijq) {
    return 0;
}

/* 
 * TODO: you must implement this function.
 * Hint:
 * - look at how the ipc_jobqueue is allocated in ipc_jobqueue_new
 */
void ipc_jobqueue_delete(ipc_jobqueue_t* ijq) {
    return;
}
