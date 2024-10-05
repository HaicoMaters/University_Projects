/******** DO NOT EDIT THIS FILE ********/
#include <stdio.h>
#include <string.h>
#include <stdlib.h>
#include <errno.h>
#include "rft_client_logging.h"

#define MIN_BAD_STATE PS_BAD_SOCKET

/* "private/static" function declarations */

/* output standard header to stream */
static void log_header(char* src_file, int src_line, protocol_t* proto);

/* output for normal states */
static void log_init(protocol_t* proto);
static void log_data_read(protocol_t* proto);
static void log_send_data(protocol_t* proto);
static void log_meta_sent(protocol_t* proto);
static void log_data_sent(protocol_t* proto);
static void log_tfr_ready(protocol_t* proto);
static void log_socktout_set(protocol_t* proto);
static void log_start_send(protocol_t* proto);
static void log_no_ack(protocol_t* proto);
static void log_ack_wait(protocol_t* proto);
static void log_ack_recv(protocol_t* proto);
static void log_server_ok(protocol_t* proto);
static void log_tfr_complete(protocol_t* proto);
static void log_tfr_mismatch(protocol_t* proto);
static void log_empty_file(protocol_t* proto);
/* output for error states */
static void bad_ack_sq_err(protocol_t* proto);
static void exceed_retry_err(protocol_t* proto);
static void log_err(protocol_t* proto);
static void bad_log_file(protocol_t* proto);
static void bad_log_line(protocol_t* proto);

/* array of output function pointers indexed by non-error protocol state */
static void (*log_protocol_state[])(protocol_t*) = {
    log_init,           // PS_INIT
    log_send_data,      // PS_RESEND_DATA
    log_tfr_ready,      // PS_TFR_READY
    log_no_ack,         // PS_NO_ACK
    log_start_send,     // PS_START_SEND
    NULL,               // undefined
    log_meta_sent,      // PS_META_SENT
    NULL,               // undefined
    log_data_read,      // PS_DATA_READ
    NULL,               // undefined
    log_send_data,      // PS_SEND_DATA
    NULL,               // undefined
    log_data_sent,      // PS_DATA_SENT
    NULL,               // undefined
    log_socktout_set,   // PS_SOCKTOUT_SET
    NULL,               // undefined
    log_ack_wait,       // PS_ACK_WAIT
    NULL,               // undefined
    log_ack_recv,       // PS_ACK_RECV
    NULL,               // undefined
    log_server_ok,      // PS_SERVER_OK
    NULL,               // undefined
    log_tfr_complete,   // PS_TFR_COMPLETE
    NULL,               // undefined
    log_tfr_mismatch,   // PS_TFR_MISMATCH
    NULL,               // undefined
    log_empty_file,     // PS_EMPTY_FILE
    NULL,               // undefined
    log_err,            // PS_BAD_SOCKET
    NULL,               // undefined
    log_err,            // PS_BAD_SERVER
    NULL,               // undefined
    log_err,            // PS_BAD_META
    NULL,               // undefined
    log_err,            // PS_BAD_READ
    NULL,               // undefined
    log_err,            // PS_BAD_SEND
    NULL,               // undefined
    log_err,            // PS_BAD_ACK
    NULL,               // undefined
    bad_ack_sq_err,     // PS_BAD_ACK_SQ
    NULL,               // undefined
    log_err,            // PS_BAD_S_SIZE
    NULL,               // undefined
    log_err,            // PS_BAD_S_FAM
    NULL,               // undefined
    log_err,            // PS_BAD_S_PORT
    NULL,               // undefined
    log_err,            // PS_BAD_S_ADDR
    NULL,               // undefined
    exceed_retry_err,   // PS_EXCEED_RETRY
    NULL,               // undefined
    log_err,            // PS_BAD_SOCKTOUT
    NULL,               // undefined
    bad_log_file,       // PS_BAD_LOG_FILE
    NULL,               // undefined
    bad_log_line        // PS_BAD_LOG_LINE
};

/* array of state strings for output indexed by protocol state */
static char* state_str[] = {
    "PS_INIT",          // PS_INIT
    "PS_RESEND_DATA",   // PS_RESEND_DATA
    "PS_TFR_READY",     // PS_TFR_READY
    "PS_NO_ACK",        // PS_NO_ACK
    "PS_START_SEND",    // PS_START_SEND
    NULL,               // undefined
    "PS_META_SENT",     // PS_META_SENT
    NULL,               // undefined
    "PS_DATA_READ",     // PS_DATA_READ
    NULL,               // undefined
    "PS_SEND_DATA",     // PS_SEND_DATA
    NULL,               // undefined
    "PS_DATA_SENT",     // PS_DATA_SENT
    NULL,               // undefined
    "PS_SOCKTOUT_SET",  // PS_SOCKTOUT_SET
    NULL,               // undefined
    "PS_ACK_WAIT",      // PS_ACK_WAIT
    NULL,               // undefined
    "PS_ACK_RECV",      // PS_ACK_RECV
    NULL,               // undefined
    "PS_SERVER_OK",     // PS_SERVER_OK
    NULL,               // undefined
    "PS_TFR_COMPLETE",  // PS_TFR_COMPLETE
    NULL,               // undefined
    "PS_TFR_MISMATCH",  // PS_TFR_MISMATCH
    NULL,               // undefined
    "PS_EMPTY_FILE",    // PS_EMPTY_FILE
    NULL,               // undefined
    "PS_BAD_SOCKET",    // PS_BAD_SOCKET
    NULL,               // undefined
    "PS_BAD_SERVER",    // PS_BAD_SERVER
    NULL,               // undefined
    "PS_BAD_META",      // PS_BAD_META
    NULL,               // undefined
    "PS_BAD_READ",      // PS_BAD_READ
    NULL,               // undefined
    "PS_BAD_SEND",      // PS_BAD_SEND
    NULL,               // undefined
    "PS_BAD_ACK",       // PS_BAD_ACK
    NULL,               // undefined
    "PS_BAD_ACK_SQ",    // PS_BAD_ACK_SQ
    NULL,               // undefined
    "PS_BAD_S_SIZE",    // PS_BAD_S_SIZE
    NULL,               // undefined
    "PS_BAD_S_FAM",     // PS_BAD_S_FAM
    NULL,               // undefined
    "PS_BAD_S_PORT",    // PS_BAD_S_PORT
    NULL,               // undefined
    "PS_BAD_S_ADDR",    // PS_BAD_S_ADDR
    NULL,               // undefined
    "PS_EXCEED_RETRY",  // PS_EXCEED_RETRY
    NULL,               // undefined
    "PS_BAD_SOCKTOUT",  // PS_BAD_SOCKTOUT
    NULL,               // undefined
    "PS_BAD_LOG_FILE",  // PS_BAD_LOG_FILE
    NULL,               // undefined
    "PS_BAD_LOG_LINE"   // PS_BAD_LOG_LINE
};

/* array of error message strings for output indexed by error states 
 * (starting at PS_BAD_SOCKET) - NULL if odd index or have custom output
 * function
 */
static char* err_msg[] = {
    "Error opening socket",                 // PS_BAD_SOCKET
    NULL,
    "Error interpreting server address",    // PS_BAD_SERVER
    NULL,
    "Error sending metadata",               // PS_BAD_META
    NULL,
    "Error reading the input file",         // PS_BAD_READ
    NULL,
    "Error in send (bytes sent do not match segment size)", // PS_BAD_SEND
    NULL,
    "ACK size does not match segment size", // PS_BAD_ACK
    NULL,
    NULL,                                   // PS_BAD_ACK_SQ
    NULL,
    "ACK server address structure size is incorrect",   // PS_BAD_S_SIZE
    NULL,
    "ACK server family does not match",     // PS_BAD_S_FAM
    NULL,
    "ACK server port does not match",       // PS_BAD_S_PORT
    NULL,
    "ACK server addr does not match",       // PS_BAD_S_ADDR
    NULL,
    NULL,                                   // PS_EXCEED_RETRY
    NULL,                               
    "Could not set the socket timeout",     // PS_BAD_SOCKTOUT
    NULL,                               
    NULL,                                   // PS_BAD_LOG_FILE
    NULL,
    NULL,                                   // PS_BAD_LOG_LINE
};

/* implementation of "public" functions declared in rft_client_logging.h */

void exit_err(char* src_file, int src_line, protocol_t* proto) {
    proto->close(proto);

    proto->log = stderr;

    log_header(src_file, src_line, proto);

    fprintf(proto->log, "ERROR: ");

    if (proto->state < MIN_BAD_STATE) {
        log_err(proto);
    } else {
        proto->err_msg = err_msg[proto->state - MIN_BAD_STATE];
        log_protocol_state[proto->state](proto);
    }

    log_separator(proto);
    
    exit(EXIT_FAILURE);
}

void log_protocol(char* src_file, int src_line, protocol_t* proto) {
    if (!proto->log)
        return;
        
    if (!src_file || !src_file[0]) {
        proto->state = PS_BAD_LOG_FILE;
        exit_err(src_file, src_line, proto);
    }
    
    if (src_line < 0) {
        proto->state = PS_BAD_LOG_LINE;
        exit_err(src_file, src_line, proto);
    }

    log_header(src_file, src_line, proto);
    
    log_protocol_state[proto->state](proto);

    log_separator(proto);

    fflush(proto->log);
}    

void log_separator(protocol_t* proto) {
    if (proto->log)
        fprintf(proto->log, 
                "----------------------------------------------------------"
                "---------------------\n");
}

/* implementation of "private/static" functions */

static void bad_ack_sq_err(protocol_t* proto) {
    fprintf(proto->log,
        "ACK sequence number %d does not match data sequence number %d\n",
        proto->ack.sq, proto->data.sq);
}

static void exceed_retry_err(protocol_t* proto) {
    fprintf(proto->log,
        "Consecutive retry limit exceeded, max_retries: %d, curr_retry: %d\n",
        proto->max_retries, proto->curr_retry);
}

static void log_err(protocol_t* proto) {
    if (errno) {
        fprintf(proto->log, "[%s]\n%s\n", strerror(errno), proto->err_msg);
    } else {
        fprintf(proto->log, "%s\n", proto->err_msg);
    }    
}

static void bad_log_file(protocol_t* proto) {
    fprintf(proto->log, "%s", 
        "Protocol source file is null or empty.\n"
        "It should be set to the __FILE__ that contains the log statement\n");
}

static void bad_log_line(protocol_t* proto) {
    fprintf(proto->log, "%s",
        "Protocol source file line is not set or is invalid.\n"
        "It should be the __LINE__ where the log statement appears\n");
}

static void log_header(char* src_file, int src_line, protocol_t* proto) {
    log_separator(proto);
    fprintf(proto->log, "CLIENT [%s:%d:state:%s]\n", src_file, src_line, 
        state_str[proto->state]);
}

static void log_init(protocol_t* proto) {
    fprintf(proto->log,"Initialised for %s transfer of file: %s to file: %s\n"
        "on server: %s:%d with loss probality: %f\n", proto->tfr_mode, 
        proto->in_fname, proto->out_fname, proto->server_addr, 
        proto->server_port, proto->loss_prob);
}

static void log_tfr_ready(protocol_t* proto) {
    fprintf(proto->log, 
        "Opened file: %s (%ld bytes), socket and server set.\n",
        proto->in_fname, (long) proto->fsize);
    fprintf(proto->log, "Client ready for transfer\n");
}

static void log_data_read(protocol_t* proto) {
    fprintf(proto->log, "Data segment read from %s input file\n",
        proto->in_fname);
}

static void log_start_send(protocol_t* proto) {
    fprintf(proto->log, "Start sending file in %s mode\n",
        proto->tfr_mode);
}

static void log_send_data(protocol_t* proto) {
    static char* prefix[] = { "Sending", "Resending", };
    fprintf(proto->log, "%s segment with sq: %d, file data: %zd, "
        "checksum: %d\ncurrent retry: %d, max retries allowed: %d\n", 
        prefix[proto->state % 2], proto->data.sq, proto->data.file_data, 
        proto->data.checksum, proto->curr_retry, proto->max_retries);

    if (proto->data.payload[0])
        fprintf(proto->log, "payload:\n%s\n", proto->data.payload);
}

static void log_meta_sent(protocol_t* proto) {
    fprintf(proto->log, "Metadata successfully sent\n");
}

static void log_data_sent(protocol_t* proto) {
    fprintf(proto->log, "Data successfully sent\n");
}

static void log_socktout_set(protocol_t* proto) {
    fprintf(proto->log, "Socket timeout successfully set\n");
}

static void log_server_ok(protocol_t* proto) {
    fprintf(proto->log, "Server in ack verified\n");
}

static void log_ack_wait(protocol_t* proto) {
    fprintf(proto->log, "Waiting for an ACK\n");
}

static void log_ack_recv(protocol_t* proto) { 
    fprintf(proto->log, "ACK with sq: %d received\n", proto->ack.sq);
        
    if (proto->ack.sq != proto->data.sq) {
        fprintf(proto->log, "but != data.sq: %d\n", proto->data.sq);
    }
}

static void log_no_ack(protocol_t* proto) { 
    fprintf(proto->log, ">>>> ACK timeout for segment with sq: %d <<<<\n",
        proto->data.sq);
}

static void log_tfr_complete(protocol_t* proto) {
    fprintf(proto->log, "Transfer complete with expected bytes transferred\n"
        "(bytes transferred == file size)\n");
    fprintf(proto->log, "Total segments sent: %d, including: %d resends\n", 
        proto->total_segments, proto->resent_segments);
    fprintf(proto->log, 
        "%zd bytes sent for transfer of file: %s of size: %ld\n",
        proto->total_file_data, proto->in_fname, (long) proto->fsize);
}

static void log_tfr_mismatch(protocol_t* proto) {
    fprintf(proto->log, 
        "Transfer completed with a mismatch between bytes transferred\n"
        "and file size\n");
    fprintf(proto->log, "Total segments sent: %d, including: %d resends\n", 
        proto->total_segments, proto->resent_segments);
    fprintf(proto->log, 
        "%zd bytes sent for transfer of file: %s of size: %ld\n",
        proto->total_file_data, proto->in_fname, (long) proto->fsize);
}

static void log_empty_file(protocol_t* proto) {
    fprintf(proto->log,
        "Input file: %s is empty (0 bytes)\n", proto->in_fname); 
    fprintf(proto->log, "No further data sent after sending metadata. "
            "Protocol terminated\n");
}
