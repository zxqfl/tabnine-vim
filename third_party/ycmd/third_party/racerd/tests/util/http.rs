use std::ops::Deref;
use std::fs::File;
use std::io::{Write, Read};

use libracerd::Config;
use libracerd::engine::{Racer, SemanticEngine};

use hyper::method::Method;

/// Smart pointer for libracerd http server.
///
/// TestServer automatically closes the underlying server when going out of scope.
pub struct TestServer {
    inner: ::libracerd::http::Server
}

impl TestServer {
    pub fn new(secret_file: Option<String>) -> TestServer {
        let engine = Racer::new();
        let config = Config {
            secret_file: secret_file,
            print_http_logs: true,
            .. Default::default()
        };

        engine.initialize(&config).unwrap();

        TestServer {
            inner: ::libracerd::http::serve(&config, engine).unwrap()
        }
    }
}

impl Deref for TestServer {
    type Target = ::libracerd::http::Server;
    fn deref(&self) -> &::libracerd::http::Server {
        &self.inner
    }
}

impl Drop for TestServer {
    fn drop(&mut self) {
        self.inner.close().unwrap();
    }
}

pub trait UrlBuilder {
    /// Given a /url/path, return a full http URL.
    fn url(&self, path: &str) -> String;
}

impl UrlBuilder for TestServer {
    fn url(&self, path: &str) -> String {
        format!("http://{}{}", self.addr(), path)
    }
}

pub fn with_server<F>(mut func: F) where F: FnMut(&TestServer) -> () {
    func(&TestServer::new(None));
}

pub fn with_hmac_server<F>(secret: &str, mut func: F) where F: FnMut(&TestServer) -> () {
    // Make a temp file unique to this test
    let thread = ::std::thread::current();
    let taskname = thread.name().unwrap();
    let s = taskname.replace("::", "_");
    let mut p = "secretfile.".to_string();
    p.push_str(&s[..]);


    {
        let mut f = File::create(&p[..]).unwrap();
        f.write_all(secret.as_bytes()).unwrap();
        f.flush().unwrap();
    }

    func(&TestServer::new(Some(p)));
}


pub fn request_str(method: Method, url: &str, data: Option<&str>)
-> ::hyper::Result<Option<String>> {
    use ::hyper::header;
    use ::hyper::status::StatusClass;
    use ::hyper::Client;

    let mut body = String::new();

    let client = Client::new();
    println!("url: {}", url);

    let mut res = match data {
        Some(inner) => {
            let builder = client.request(method, url)
                                .header(header::Connection::close())
                                .header(header::ContentType::json())
                                .body(inner);

            try!(builder.send())
        },
        None => {
            let builder = client.request(method, url)
                                .header(header::Connection::close());

            try!(builder.send())
        }
    };

    Ok(match res.status.class() {
        StatusClass::Success => {
            try!(res.read_to_string(&mut body));
            Some(body)
        },
        _ => None
    })
}

#[test]
fn server_url_builder() {
    with_server(|server| {
        let url = server.url("/definition");
        assert!(url.starts_with("http://"));
        assert!(url.ends_with("/definition"));
    });
}
