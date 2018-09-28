use rustc_serialize::hex::ToHex;

use crypto::hmac::Hmac;
use crypto::mac::Mac;
use crypto::sha2::Sha256;

#[macro_use]
pub mod http;

pub fn hmac256(secret: &[u8], data: &[u8]) -> Vec<u8> {
    let mut hmac = Hmac::new(Sha256::new(), secret);

    let len = hmac.output_bytes();
    let mut result = Vec::with_capacity(len);

    for _ in 0..len {
        result.push(0);
    }

    hmac.input(data);
    hmac.raw_result(&mut result[..]);

    result
}

pub fn request_hmac(secret_str: &str, method: &str, path: &str, body: &str) -> String {
    // hmac(hmac(GET) + hmac(/ping) + hmac())
    let secret = secret_str.as_bytes();

    let method_hmac = hmac256(secret, method.as_bytes());
    let path_hmac = hmac256(secret, path.as_bytes());
    let body_hmac = hmac256(secret, body.as_bytes());

    let mut meta = Hmac::new(Sha256::new(), secret);
    meta.input(&method_hmac[..]);
    meta.input(&path_hmac[..]);
    meta.input(&body_hmac[..]);

    let len = meta.output_bytes();
    let mut result = Vec::with_capacity(len);

    for _ in 0..len {
        result.push(0);
    }

    meta.raw_result(&mut result[..]);
    result.to_hex()
}
