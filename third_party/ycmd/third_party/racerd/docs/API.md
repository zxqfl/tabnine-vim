HTTP/JSON API
=============

The example responses throughout this document were generated with a
`--rust-src-path` value of `/Users/jwilm/rs/std/stable`. Copy and paste cURL
invocations are given throughout. If you wish to run them, start a racerd server
using the following command.

```bash
racerd serve -l -p56773 --rust-src-path <your-rust-src-path>
```

## Overview

[`POST /find_definition`](#post-find_definition) attempts to find the definition
of the item under the cursor.

[`POST /list_completions`](#post-list_completions) attempts to generate a list of
completions given the item under the cursor.

[`GET /ping`](#get-ping) is a status endpoint indicating whether the server is
available.

### Success

The `/ping` endpoint should always return a `200 Ok` response. The semantic
analysis endpoints return either `200 Ok` or `204 No Content` in the case of a
successful request. A `204` response indicates that the operation completed
successfully, but no definition or completion was found.

### Failure

Other possible responses include `403 Forbidden` if using HMAC auth and
providing an invalid HMAC, `400 Bad Request` if your request data is malformed,
and `500 Internal Server Error` if anything else goes wrong.

## Authentication

The server may be started with HMAC authentication. To query the server when
HMAC is enabled, you must add an `x-racerd-hmac` header to your request. The
value can be computed with the formula

`hmac(secret, hmac(secret, method) + hmac(secret, path) + hmac(secret, body))`

where `hmac()` is an SHA-256 HMAC routine.

An important note is that the final output should be hex-encoded but the steps
in between should not.

A correct implementation can be found at https://github.com/jwilm/racerd/blob/master/tests/util/mod.rs#L26

## Common types

### Buffer

A buffer is a potentially in memory representation of a file. Racerd supports
specifying a list of buffers with each [QueryRequest][] to use instead of
reading those files from disk.

| Field     | Type   | Meaning                              |
|-----------|--------|--------------------------------------|
| file_path | string | File path to associate with contents |
| contents  | string | Contents to use for given file_path  |

### QueryRequest

The `QueryRequest` type is used by both the [find definition][] and
[list completions][] endpoints.

| Field     | Type              | Meaning                                                                                       |
|-----------|-------------------|-----------------------------------------------------------------------------------------------|
| file_path | string            | File path for query context                                                                   |
| buffers   | Array<[Buffer][]> | List of buffers to use instead of loading from disk. Files omitted here are loaded from disk. |
| line      | number            | Cursor line for query context                                                                 |
| column    | number            | Cursor column for query context                                                               |


## POST /list_completions

`POST /list_completions` accepts the [QueryRequest][] request type.

### cURL Command

```bash
curl -H "Content-Type: application/json" -d '{"buffers":[{"file_path":"src.rs","contents":"use std::path;\nfn main() {\nlet p = path::Path::\n}\n"}],"file_path":"src.rs","line":3,"column":20}' 127.0.0.1:56773/list_completions
```

### Request

```json
{
  "buffers":[
    {
      "file_path":"src.rs",
      "contents":"use std::path;\nfn main() {\nlet p = path::Path::\n}\n"
    }
  ],
  "file_path":"src.rs",
  "line":3,
  "column":20
}
```

### Response

The response for `/list_completions` is an array of completion data objects.

| Field     | Type   | Meaning                               |
|-----------|--------|---------------------------------------|
| file_path | string | Path to file where completion found   |
| line      | number | Line in file where completion found   |
| column    | number | Column in file where completion found |
| text      | string | Text of completion                    |
| context   | string | Surrounding text of completion        |
| kind      | string | Type of completion                    |

```
[
  {
    "text":"new",
    "context":"pub fn new<S: AsRef<OsStr> + ?Sized>(s: &S) -> &Path {",
    "kind":"Function",
    "file_path":"/Users/jwilm/rs/std/stable/libstd/path.rs",
    "line":1267,
    "column":11
  }
]
```

## POST /find_definition

`POST /find_definition` accepts the [QueryRequest][] request type.

### cURL Command

```bash
curl -H "Content-Type: application/json" -d '{"buffers":[{"file_path":"src.rs","contents":"use std::path::Path;\nfn main() {\nlet p = &Path::new(\"test\")\n}\n"}],"file_path":"src.rs","line":3,"column":16}' 127.0.0.1:56773/find_definition
```

### Request

```json
{
  "buffers":[
    {
      "file_path":"src.rs",
      "contents":"use std::path::Path;\nfn main() {\nlet p = &Path::new(\"test\")\n}\n"
    }
  ],
  "file_path":"src.rs",
  "line":3,
  "column":16
}
```

### Response

| Field     | Type   | Meaning                                    |
|-----------|--------|--------------------------------------------|
| file_path | string | Path to file in which definition was found |
| text      | string | Text of found definition                   |
| line      | number | Line in file where definition was found    |
| column    | number | Column in file where definition was found  |
| context   | string | Surrounding text of definition             |
| kind      | string | Type of definition                         |

```json
{
  "file_path":"/Users/jwilm/rs/std/stable/libstd/path.rs",
  "column":11,
  "line":1267,
  "text":"new",
  "context":"pub fn new<S: AsRef<OsStr> + ?Sized>(s: &S) -> &Path {",
  "kind":"Function"
}
```


## GET /ping

The `GET /ping` endpoint is used as a status check for the server. It will
always return the same response regardless of what is sent in the body.

### cURL Command

```bash
curl 127.0.0.1:56773/ping
```

### Response

```json
{
  "pong": true
}
```

[QueryRequest]: #queryrequest
[Buffer]: #buffer
[find definition]: #post-find_definition
[list completions]: #post-list_completions
