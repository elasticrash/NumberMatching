extern crate rand;
use rand::Rng;
use std::collections::HashMap;
use std::io;
use std::io::Write;
use std::str::FromStr;
use std::sync::atomic::{AtomicUsize, Ordering};
use std::time::{SystemTime, UNIX_EPOCH};
use std::collections::HashSet;

struct Indx {
    d: HashMap<i32, Indx>,
    m: Vec<i32>,
}

static POINTER: AtomicUsize = AtomicUsize::new(1);
static SIZE: i32 = 50000;
fn main() {
    let start = print_time();
    println!("Generating Random Numbers");
    let generated_numbers = random_number_generator();
    let mut indexed_tokens: HashMap<i32, Indx> = HashMap::new();

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

fn print_duration(start: &u64, end: &u64) {
    println!(" took {:?} nanoseconds", end - start);
    println!(" took {:?} milliseconds", (end - start) / 1000000);
}

fn random_number_generator() -> Vec<i32> {
    let mut numbers: Vec<i32> = vec![];
    let mut rng = rand::thread_rng();

    let mut item = 0;
    loop {
        let random_number: i32 = rng.gen_range(1000000, 9999999);
        if !numbers.iter().any(|x| x == &random_number) {
            numbers.push(random_number);
            item += 1;
        }
        if item == SIZE {
            break;
        }
    }

    return numbers;
}

fn tokenize(num: i32, index: &mut HashMap<i32, Indx>, id: usize, level: i32) {
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
        let key = i32::from_str(&string_key).unwrap_or(0);

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
    let key = i32::from_str(&string_key).unwrap_or(0);
    let index: &mut Indx = opt.unwrap();

    if !index.d.contains_key(&key) {
        let mut new_index = Indx {
            d: HashMap::new(),
            m: vec![],
        };

        if level > 3 {
            new_index.m.push(id as i32);
        }
        index.d.insert(key, new_index);
    } else {
        let exist: &mut Indx = index.d.get_mut(&key).unwrap();
        let itr_indx = &exist.m;
        let mut itr = itr_indx.iter();
        let duplicate = itr.any(|x| x == &(id as i32));

        if duplicate {
            if level > 3 {
                exist.m.push(id as i32);
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

fn enter_value(set: &HashMap<i32, Indx>, num: &Vec<i32>) {
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

fn number_search(search: &str, set: &HashMap<i32, Indx>, num: &Vec<i32>) {
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
    let key = i32::from_str(&string_key).unwrap_or(0);
    let mut current = set.get(&key).unwrap();

    let mut broken = false;

    for x in 1..search_chars.len() {
        let t = search_chars[x].to_string();
        let local_key = i32::from_str(&t).unwrap_or(0);
        if !current.d.contains_key(&local_key) {
            broken = true;
            break;
        }

        current = current.d.get(&local_key).unwrap();
    }

    if !broken {
         println!("results found: {:?} ", current.m.len());
         current.m.iter().cloned().collect::<HashSet<_>>().iter().map(|x| num[*x as usize]).for_each(|x| println!("{}", x));
    } else {
        println!("no matches found");
    }
}
