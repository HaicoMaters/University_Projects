/*
 * Replace the following string of 0s with your student number
 * 200972727
 */
#include <fcntl.h>          /* For O_* constants */
#include <sys/stat.h>       /* For mode constants */
#include <semaphore.h>
#include "shobject_name.h"
#include "sem_jobqueue.h"

/* 
 * DO NOT EDIT the following declarations that are used to detect
 * failures in semaphore setup
 */
#define SEM_NEW_FAIL        000
#define MUTEX_SEM_SUCCESS   001
#define FULL_SEM_SUCCESS    002
#define EMPTY_SEM_SUCCESS   004
#define ALL_SEM_SUCCESS     007

/* 
 * DO NOT EDIT the following semaphore names.
 */
static const char* sem_mutex_label = "sjq.mutex";
static const char* sem_full_label = "sjq.full";
static const char* sem_empty_label = "sjq.empty";

/* 
 * DO NOT EDIT the private helper function sem_new for creating new
 * semaphores at initialisation
 */
static int sem_new(sem_t** sem, const char* sem_label, int init_value, 
    int success) {
    char sem_name[MAX_NAME_SIZE];
    
    shobject_name(sem_label, sem_name);
    
    sem_t* new_sem = sem_open(sem_name, O_CREAT, S_IRWXU, init_value);
    
    if (new_sem == SEM_FAILED)
        return SEM_NEW_FAIL;
    
    *sem = new_sem;
        
    return success;
}

/* 
 * DO NOT EDIT the private helper function sem_delete for closing semaphores
 * when a sem_jobqueue is deleted.
 */
static void sem_delete(sem_t* sem, const char* sem_label) {
    char sem_name[MAX_NAME_SIZE];
    sem_close(sem);
    shobject_name(sem_label, sem_name);
    sem_unlink(sem_name);
}

/* 
 * DO NOT EDIT sem_jobqueue_new that creates a new sem_jobqueue_t
 * and associated semaphores.
 * You will need to look at this function to see what needs to be deleted, 
 * freed or closed by sem_jobqueue_delete
 */
sem_jobqueue_t* sem_jobqueue_new(proc_t* proc) {
    sem_jobqueue_t* sjq = (sem_jobqueue_t*) malloc(sizeof(sem_jobqueue_t));

    if (!sjq)
        return NULL;
        
    sjq->ijq = ipc_jobqueue_new(proc);   // delays all but init process
    
    if (!sjq->ijq) {
        free(sjq);
        return NULL;
    }
    
    int r = sem_new(&sjq->mutex, sem_mutex_label, 1, MUTEX_SEM_SUCCESS);
    
    if (r != MUTEX_SEM_SUCCESS) {
        ipc_jobqueue_delete(sjq->ijq);
        free(sjq);
        return NULL;
    }
    
    sem_wait(sjq->mutex);
    
    r |= sem_new(&sjq->full, sem_full_label, 0, FULL_SEM_SUCCESS)
            | sem_new(&sjq->empty, sem_empty_label,
                ipc_jobqueue_space(sjq->ijq), EMPTY_SEM_SUCCESS);
    
    if (r & ALL_SEM_SUCCESS) {
        sem_post(sjq->mutex);
        return sjq;    // all succeeded
    }
    
    // mutex failures    
    if (r & FULL_SEM_SUCCESS)
        sem_delete(sjq->full, sem_full_label);

    if (r & EMPTY_SEM_SUCCESS)
        sem_delete(sjq->empty, sem_empty_label);

    sem_post(sjq->mutex);
    sem_delete(sjq->mutex, sem_mutex_label);
    ipc_jobqueue_delete(sjq->ijq);
    free(sjq);
                
    return NULL;
}

/* 
 * TODO: you must implement this function according to the specification in
 * sem_jobqueue.h
 */
job_t* sem_jobqueue_dequeue(sem_jobqueue_t* sjq, job_t* dst) {
	if(sjq != NULL){
		int success = sem_wait(sjq -> full);
		if(success != 0){return NULL;} //sem_wait returns 0 on success
		success = sem_wait(sjq -> mutex);
		if(success != 0){return NULL;}
		dst = ipc_jobqueue_dequeue(sjq -> ijq, dst);
		sem_post(sjq -> mutex);
		sem_post(sjq -> empty);
		do_noncritical_work(sjq -> ijq -> proc);
		return dst;
		
	}
    return NULL;
}

/* 
 * TODO: you must implement this function according to the specification in
 * sem_jobqueue.h
 */
void sem_jobqueue_enqueue(sem_jobqueue_t* sjq, job_t* job) {
	if(sjq != NULL){
		int success = sem_wait(sjq -> empty);
		if(success != 0){return;} //sem_wait returns 0 on success
		success = sem_wait(sjq -> mutex);
		if(success != 0){return;}
		ipc_jobqueue_enqueue(sjq -> ijq, job);
		sem_post(sjq -> mutex);
		sem_post(sjq -> full);
		do_noncritical_work(sjq -> ijq -> proc);
	}
    return;
}

/* 
 * TODO: you must implement this function according to the specification in
 * sem_jobqueue.h
 */
bool sem_jobqueue_is_empty(sem_jobqueue_t* sjq) {
	if(sjq != NULL){
		int success = sem_wait(sjq -> mutex);
		if (success == 0){ //sem_wait returns 0 on success
			bool is_empty = ipc_jobqueue_is_empty(sjq -> ijq);
			sem_post(sjq -> mutex);
			do_noncritical_work(sjq -> ijq -> proc);
			return is_empty;
		}
	}
    return true;
}

/* 
 * TODO: you must implement this function according to the specification in
 * sem_jobqueue.h
 */
bool sem_jobqueue_is_full(sem_jobqueue_t* sjq) {
	if(sjq != NULL){
		int success = sem_wait(sjq -> mutex);
		if (success == 0){ //sem_wait returns 0 on success
			bool is_full = ipc_jobqueue_is_full(sjq -> ijq);
			sem_post(sjq -> mutex);
			do_noncritical_work(sjq -> ijq -> proc);
			return is_full;
		}
	}
    return true;
}

/* 
 * TODO: you must implement this function according to the specification in
 * sem_jobqueue.h
 */
job_t* sem_jobqueue_peek(sem_jobqueue_t* sjq, job_t* dst) {
	if(sjq != NULL){
		int success = sem_wait(sjq -> mutex);
		if (success == 0){ //sem_wait returns 0 on success
			dst = ipc_jobqueue_peek(sjq -> ijq, dst);
			sem_post(sjq -> mutex);
			do_noncritical_work(sjq -> ijq -> proc);
			return dst;
		}
	}
    return NULL;
}

/* 
 * TODO: you must implement this function according to the specification in
 * sem_jobqueue.h
 */
int sem_jobqueue_size(sem_jobqueue_t* sjq) {
	if(sjq != NULL){
		int success = sem_wait(sjq -> mutex);
		if (success == 0){ //sem_wait returns 0 on success
			int size = ipc_jobqueue_size(sjq -> ijq);
			sem_post(sjq -> mutex);
			do_noncritical_work(sjq -> ijq -> proc);
			return size;
		}
	}
    return 0;
}

/* 
 * TODO: you must implement this function according to the specification in
 * sem_jobqueue.h
 */
int sem_jobqueue_space(sem_jobqueue_t* sjq) {
	if(sjq != NULL){
		int success = sem_wait(sjq -> mutex);
		if (success == 0){ //sem_wait returns 0 on success
			int space = ipc_jobqueue_space(sjq -> ijq);
			sem_post(sjq -> mutex);
			do_noncritical_work(sjq -> ijq -> proc);
			return space;
		}
	}
    return 0;
}

/* 
 * TODO: you must implement this function according to the specification in
 * sem_jobqueue.h
 * Hint:
 * - look at what is allocated and/or opened in sem_jobqueue_new and in what 
 *      order
 */
void sem_jobqueue_delete(sem_jobqueue_t* sjq) {
	if(sjq){
		sem_delete(sjq->mutex, sem_mutex_label);
		sem_delete(sjq->empty, sem_empty_label);
		sem_delete(sjq->full, sem_full_label);
		ipc_jobqueue_delete(sjq -> ijq);
		free(sjq);
	}
    return;
}