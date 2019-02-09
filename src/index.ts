import * as readline from 'readline';
import * as fs from 'fs';

declare global {
    interface Array<T> {
        forEachPair(callback: Function, thisArg?: any): any;
    }
}

const numbers: any[] = [
]

for (let j = 0; j < 4000; j++) {
    numbers.push(Math.floor(Math.random() * 9000000) + 1000000);
}

let build = 0;

function tokenize(n: string | number, collection: any, id: any, level = 1) {
    const charArray: any = n.toString().split('');
    const nextStep = n.toString().substr(1)

    charArray.forEachPair((pair: any) => {
        const hash = `${pair[0]}${pair[1]}`;
        const newlevel = level + 1;

        if (collection[hash] === undefined) {
            collection[hash] = {};
            collection[hash].matches = [];
            if (!exists(collection[hash].matches, id.toString(16))) {
                collection[hash].matches.push(id.toString(16));
            }
            tokenize(nextStep, collection[hash], id, newlevel);
        } else {
            if (!exists(collection[hash].matches, id.toString(16))) {
                collection[hash].matches.push(id.toString(16));
            }
            tokenize(nextStep, collection[hash], id, newlevel);
        }
    });

    if (level === 1) {
        build++;
        process.stdout.write(`building index ${build}/${numbers.length}\r`);
    }
}

function exists(collection: any[], value: string) {
    const exists = collection.findIndex(x => x === value);
    return exists !== -1 ? true : false;
}

Array.prototype.forEachPair = function (callback: Function, thisArg: any) {

    var T, k;

    if (this == null) {
        throw new TypeError(' this is null or not defined');
    }

    var O = Object(this);

    var len = O.length >>> 0;

    if (typeof callback !== "function") {
        throw new TypeError(callback + ' is not a function');
    }

    if (arguments.length > 1) {
        T = thisArg;
    }

    k = 1;

    while (k < len) {

        var kValue = [];

        if (k in O) {

            kValue[0] = O[k - 1];
            kValue[1] = O[k];

            callback.call(T, kValue, k, O);
        }
        k++;
    }
};

function numberSearch(search: string) {
    const charArray: any = search.toString().split('');
    const tokens: any[] = [];

    charArray.forEachPair((pair: any) => {
        const keypair = `${pair[0]}${pair[1]}`
        tokens.push(keypair);
    });

    let result = index;
    tokens.forEach(element => {
        if (result != null && result[element]) {
            result = result[element];
        } else {
            result = null;
        }
    });

    if (result !== null) {
        const getresults: any[] = [];
        result.matches.forEach((element: string) => {
            getresults.push(numbers[parseInt(element, 16)]);
        });
        return getresults;
    } else {
        return "no matches";
    }
}

console.log(`creating index`);
console.log(new Date().toUTCString());
const index: any = {};
numbers.forEach((element, i) => {
    tokenize(element, index, i);
});
console.log(`index created`);
console.log(new Date().toUTCString());

fs.writeFileSync('./data.json', JSON.stringify(index), 'utf-8');

console.log('type any number and press enter / or ctrl-c to exit');

readline.emitKeypressEvents(process.stdin);
if (process.stdin && process.stdin.setRawMode) {
    process.stdin.setRawMode(true);
    let search = '';
    process.stdin.on('keypress', (str, key) => {
        if (key.ctrl && key.name === 'c') {
            process.exit();
        } if (key.name === 'return') {
            console.log();
            const start = Date.now();
            console.log(`match: ${numberSearch(search)}`);
            console.log(`search took ${Date.now() - start} ms`);
            search = '';
            console.log("enter another number");
        } else {
            process.stdout.write(str);
            search += str;
        }
    });
}
