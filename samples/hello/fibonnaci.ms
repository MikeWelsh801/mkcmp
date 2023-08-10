print("Give me a number and I will tell you the fibonnaci number of that index.")

let number = int(input())
let result = fib(number)
print("Answer: " + string(result))


fun fib(n: int): int {
    if n == 0 || n == 1 
        return n 

    return fib(n - 1) + fib(n - 2)
}
