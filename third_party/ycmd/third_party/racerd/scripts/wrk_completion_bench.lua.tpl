wrk.method = "POST"
wrk.path   = "/list_completions"
wrk.body   = "{ \"buffers\" : [ { \"file_path\" : \"src.rs\" , \"contents\" : \"\\nfn main() {\\n[[[completion]]]\\n}\\n\" } ] , \"file_path\" : \"src.rs\" , \"line\" : 3 , \"column\" : [[[column]]] }"
wrk.headers["Content-Type"] = "application/json"
