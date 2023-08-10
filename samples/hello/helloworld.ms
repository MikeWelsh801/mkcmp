print("What is your name?")

let name = input()
hello(name)

fun hello(name: string) {
    if name == "Greg"
        print("I don't like you, " + name + ".")
    else
        print("Hello, " + name + "!")
}
