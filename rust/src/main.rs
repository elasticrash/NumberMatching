extern crate rand;
use rand::Rng;

fn main() {
    println!("Hello, world!");
    println!("{:?}", random_number_generator());
}

fn random_number_generator() -> Vec<i32> {
    let mut numbers: Vec<i32> = vec![];
    let mut rng = rand::thread_rng();

    for x in 0..100 {
        numbers.push(rng.gen_range(1000000, 9999999));
    }

    return numbers;
}
