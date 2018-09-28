#!/usr/bin/env python
# vim: ts=4 sw=4 et cc=80 tw=79

import subprocess
import tempfile

import os
from os import path

# Support overriding RACERD. Assume racerd is on path by default.
if os.environ.has_key('RACERD'):
    RACERD = os.environ['RACERD']
else:
    RACERD = 'racerd'

def get_scripts_dir():
    """
    Return absolute path of scripts directory
    """
    return path.abspath(path.dirname(__file__))

def write_wrk_script(completion_string):
    """
    Read the wrk template script, replace the completion string and column
    number, write a temporary file, and return the file path
    """

    # Read the file
    tpl_file = open(get_wrk_template_path(), 'r')
    tpl = tpl_file.read()

    # Replace template params
    tpl = tpl.replace('[[[completion]]]', completion_string)
    tpl = tpl.replace('[[[column]]]', str(len(completion_string)))

    # Write temp file
    lua_file_fd, lua_file_path = tempfile.mkstemp(suffix='.lua', text=True)
    lua_file = os.fdopen(lua_file_fd, 'w')
    lua_file.write(tpl)
    lua_file.close()

    return lua_file_path


def get_wrk_template_path():
    """
    A template lua script for wrk is used to generate completion requests. This
    function returns the path to the template script
    """
    return path.join(get_scripts_dir(), 'wrk_completion_bench.lua.tpl')

def start_racerd():
    """
    Spawn a racerd process on random port. Returns the process and host string.
    # Example
    (process, host) = start_racerd()
    """
    process = subprocess.Popen(
        [RACERD, 'serve', '--secret-file=hah', '--port=0'],
        stdout = subprocess.PIPE
        )

    racerd_listen_line = process.stdout.readline()
    racerd_host = racerd_listen_line.split(' ')[3]

    return (process, racerd_host.strip())


def run_wrk(script_path, host):
    """
    Spawn a `wrk` process with 1 thread, 1 connection, and run for 1 second.
    These should probably be changed to environment variables in the future.
    """
    base_url = 'http://' + host
    output = subprocess.check_output(
        ['wrk', '-t1', '-c1', '-d1s', '-s', script_path, base_url]
        )

    lines = output.splitlines()

    # Line 3 in the second column by whitespace has the average request length.
    latency_line = lines[3]
    latency_avg = latency_line.split()[1]

    return latency_avg

def print_report(completion_str, latency_avg):
    """
    Print a report for given run
    """
    print 'Completion for "' + completion_str + '" averaged ' + latency_avg

def bench_completion(completion_str):
    """
    Start racerd and run wrk for a given completion string
    """

    # Write wrk script for this completion
    wrk_script_path = write_wrk_script(completion_str)

    # Start racerd and run wrk
    process, host = start_racerd()
    latency_avg = run_wrk(wrk_script_path, host)

    # Print a report
    print_report(completion_str, latency_avg)

    # cleanup
    process.terminate()
    os.remove(wrk_script_path)


completions = [
    'use ::std::',
    'use ::std::io::',
    'use ::std::path::',
    'use ::std::path::P'
]

for c in completions:
    bench_completion(c)

