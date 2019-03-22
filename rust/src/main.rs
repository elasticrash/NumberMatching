extern crate rand;
use rand::Rng;
use std::collections::HashMap;
use std::str::FromStr;
use std::time::{SystemTime, UNIX_EPOCH};

struct Indx {
    d: HashMap<i32, Indx>,
    m: Vec<String>,
}

fn main() {
    let start = print_time();
    println!("Generating Random Numbers");
    let generated_numbers = random_number_generator();
    // println!("{:?}", generated_numbers);

    let mut indexed_tokens: HashMap<i32, Indx> = HashMap::new();

    for x in 0..generated_numbers.len() {
        tokenize(generated_numbers[x], &mut indexed_tokens, x, 1);
    }
    let end = print_time();
    println!("took {:?} nanoseconds to create the index", end - start);
    println!(
        "took {:?} milliseconds to create the index",
        (end - start) / 1000000
    );
}

fn random_number_generator() -> Vec<i32> {
    let mut numbers: Vec<i32> = vec![];
    let mut rng = rand::thread_rng();

    for _x in 0..10000 {
        numbers.push(rng.gen_range(1000000, 9999999));
    }

    return numbers;
}

fn tokenize(num: i32, index: &mut HashMap<i32, Indx>, id: usize, level: i32) {
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
            new_index.m.push(id.to_string());
        }
        index.d.insert(key, new_index);
    } else {
        let exist: &mut Indx = index.d.get_mut(&key).unwrap();
        let itr_indx = &exist.m;
        let mut itr = itr_indx.into_iter();
        let duplicate = itr.any(|x| x == &id.to_string());

        if duplicate {
            if level > 3 {
                exist.m.push(id.to_string());
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
