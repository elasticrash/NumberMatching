extern crate rand;
use rand::Rng;
use std::collections::HashMap;

fn main() {
    println!("Generating Random Numbers");
    let generated_numbers = random_number_generator();
    println!("{:?}", generated_numbers);

    let mut indexed_tokens:HashMap<&String, &String> = HashMap::new();

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

fn tokenize(num: i32, index: &mut HashMap<&String, &String>){
    println!("{:?}", num);
}