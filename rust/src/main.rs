extern crate rand;
use rand::Rng;
use std::collections::HashMap;
use std::str::FromStr;

struct Indx {
    d: HashMap<i32, Indx>,
    m: Vec<String>,
}

fn main() {
    println!("Generating Random Numbers");
    let generated_numbers = random_number_generator();
    println!("{:?}", generated_numbers);

    let mut indexed_tokens:HashMap<i32, Indx> = HashMap::new();

    for x in 0..generated_numbers.len() {
        tokenize(generated_numbers[x], &mut indexed_tokens);
    }
}

fn random_number_generator() -> Vec<i32> {
    let mut numbers: Vec<i32> = vec![];
    let mut rng = rand::thread_rng();

    for _x in 0..100 {
        numbers.push(rng.gen_range(1000000, 9999999));
    }

    return numbers;
}

fn tokenize(num: i32, index: &mut HashMap<i32, Indx>){
    let char_vec: Vec<char> = num.to_string().chars().collect();

    for x in 0..char_vec.len(){
        let digit = char_vec[x];
        let string_key = digit.to_string();
        let key = i32::from_str(&string_key).unwrap_or(0);

        if !index.contains_key(&key) {
            
            let new_index = Indx {
                d: HashMap::new(),
                m: vec![]
            };

            index.insert(key, new_index);

            if char_vec.len() - x < 4 {
                continue;
            }

            let next_step = &num.to_string()[..(x+1)];
        }
    }
}