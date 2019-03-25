extern crate quick_protobuf;
extern crate rand;
use quick_protobuf::{BytesReader, MessageRead};
use std::collections::HashMap;
use std::collections::HashSet;
use std::fs::File;
use std::io;
use std::io::prelude::*;
use std::io::Write;
use std::str::FromStr;
use std::sync::atomic::{AtomicUsize, Ordering};
use std::time::{SystemTime, UNIX_EPOCH};

struct Indx {
    d: HashMap<i64, Indx>,
    m: Vec<i64>,
}

#[derive(Debug, Default, PartialEq, Clone)]
pub struct RandomNumberGenerator {
    pub random_numbers: Vec<i64>,
}

static POINTER: AtomicUsize = AtomicUsize::new(1);
static SIZE: AtomicUsize = AtomicUsize::new(0);

fn main() {
    let read = read_from_file();
    let start = print_time();
    // println!("Generating Random Numbers");
    // let generated_numbers = random_number_generator();

    let generated_numbers = read.unwrap().random_numbers;
    SIZE.fetch_add(generated_numbers.len(), Ordering::SeqCst);
    let mut indexed_tokens: HashMap<i64, Indx> = HashMap::new();

    for x in 0..generated_numbers.len() {
        if x < 10 {
            println!("{:?}", generated_numbers[x]);
        }
        tokenize(generated_numbers[x], &mut indexed_tokens, x, 1);
    }
    let end = print_time();
    print_duration(&start, &end);

    let set = &indexed_tokens;
    enter_value(&set, &generated_numbers);
}

fn read_from_file() -> Result<RandomNumberGenerator, quick_protobuf::Error> {
    let mut f = File::open("./numbers.bin")?;
    let mut bytes: Vec<u8> = vec![];
    f.read_to_end(&mut bytes);
    let mut reader = BytesReader::from_bytes(&bytes);
    let result = RandomNumberGenerator::from_reader(&mut reader, &bytes);
    return result;
}

impl<'a> MessageRead<'a> for RandomNumberGenerator {
    fn from_reader(
        r: &mut BytesReader,
        bytes: &'a [u8],
    ) -> Result<RandomNumberGenerator, quick_protobuf::Error> {
        let mut msg = Self::default();
        while !r.is_eof() {
            match r.next_tag(bytes) {
                Ok(8) => msg.random_numbers.push(r.read_int64(bytes)?),
                Ok(t) => {
                    r.read_unknown(bytes, t)?;
                }
                Err(e) => return Err(e),
            }
        }
        Ok(msg)
    }
}

fn print_duration(start: &u64, end: &u64) {
    println!(" took {:?} nanoseconds", end - start);
    println!(" took {:?} milliseconds", (end - start) / 1000000);
}

fn tokenize(num: i64, index: &mut HashMap<i64, Indx>, id: usize, level: i32) {
    if level == 1 {
        if POINTER.load(Ordering::SeqCst) % 100 == 0 {
            print!("\r building index {:?}/{:?}", POINTER, SIZE);
        }
        POINTER.fetch_add(1, Ordering::SeqCst);
    }

    let char_vec: Vec<char> = num.to_string().chars().collect();

    for x in 0..char_vec.len() {
        let digit = char_vec[x];
        let string_key = digit.to_string();
        let key = i64::from_str(&string_key).unwrap_or(0);

        if !index.contains_key(&key) {
            let new_index = Indx {
                d: HashMap::new(),
                m: vec![],
            };

            index.insert(key, new_index);
        }

        if char_vec.len() - x < 4 {
            continue;
        }

        let next_step = &num.to_string()[(x + 1)..];
        let map = index.get_mut(&key);
        populate_next_level(next_step.to_string(), map, id, level + 1);
    }
}

fn populate_next_level(step: String, opt: Option<&mut Indx>, id: usize, level: i32) {
    let sub: Vec<char> = step.chars().collect();
    if sub.len() == 0 {
        return;
    }

    let digit = sub[0];
    let string_key = digit.to_string();
    let key = i64::from_str(&string_key).unwrap_or(0);
    let index: &mut Indx = opt.unwrap();

    if !index.d.contains_key(&key) {
        let mut new_index = Indx {
            d: HashMap::new(),
            m: vec![],
        };

        if level > 3 {
            new_index.m.push(id as i64);
        }
        index.d.insert(key, new_index);
    } else {
        let exist: &mut Indx = index.d.get_mut(&key).unwrap();
        let itr_indx = &exist.m;
        let mut itr = itr_indx.iter();
        let duplicate = itr.any(|x| x == &(id as i64));

        if !duplicate {
            if level > 3 {
                exist.m.push(id as i64);
            }
        }
    }

    let previous: Option<&mut Indx> = Some(index.d.get_mut(&key).unwrap());
    let next_step = &step.to_string()[(1)..];
    populate_next_level(next_step.to_string(), previous, id, level + 1);
}

fn print_time() -> u64 {
    let start = SystemTime::now();
    let since_the_epoch = start
        .duration_since(UNIX_EPOCH)
        .expect("Time went backwards");

    let in_ms = since_the_epoch.as_secs() * 1000000000 + since_the_epoch.subsec_nanos() as u64;
    return in_ms;
}

fn read_from_stdin() -> String {
    io::stdout().flush().unwrap();
    let mut val = String::new();
    io::stdin().read_line(&mut val);
    return val;
}

fn enter_value(set: &HashMap<i64, Indx>, num: &Vec<i64>) {
    println!("type any number and press enter / or x to exit");
    let trm = read_from_stdin();
    let value = trm.trim();
    if value == "x" {
        return;
    }

    let start = print_time();
    number_search(value, &set, &num);
    let end = print_time();
    print_duration(&start, &end);

    enter_value(&set, &num);
}

fn number_search(search: &str, set: &HashMap<i64, Indx>, num: &Vec<i64>) {
    if !search.parse::<f64>().is_ok() {
        println!("string contains invalid characters");
        return;
    }
    let search_chars: Vec<char> = search.chars().collect();
    if search_chars.len() < 4 {
        println!("you need at least 4 characters to do a search");
        return;
    }

    let string_key = search_chars[0].to_string();
    let key = i64::from_str(&string_key).unwrap_or(0);
    let mut current = set.get(&key).unwrap();

    let mut broken = false;

    for x in 1..search_chars.len() {
        let t = search_chars[x].to_string();
        let local_key = i64::from_str(&t).unwrap_or(0);
        if !current.d.contains_key(&local_key) {
            broken = true;
            break;
        }

        current = current.d.get(&local_key).unwrap();
    }

    if !broken {
        let res = current
            .m
            .iter()
            .cloned()
            .collect::<HashSet<_>>()
            .iter()
            .map(|x| num[*x as usize].to_string())
            .collect::<Vec<String>>()
            .join(",");

        println!("{:?}", res);
    } else {
        println!("no matches found");
    }
}
