// Simple Interpreter in F#
// Authors: Arthur Coombes, Will Dingle, Yucheng Bian
// Date: 04/11/2024
// Reference: Peter Sestoft, Grammars and parsing with F#, Tech. Report

namespace FsharpLib
open System
open System.Collections.Generic

module Interpreter = 

    // Define the terminal type, representing all possible tokens in the language
    type terminal = 
        Add | Sub | Mul | Div | Lpar | Rpar | Mod | Pow 
        | Var of String // Represents variable names
        | Func of String // Represents variable names
        | Equ | Num of string // Numeric literals
        | E | OP of string // Operators like "cos", "tan", etc.
        | Def | Plot | Comma // Special keywords


    // Look Up Table for terminal list
    let lut = [("cos", OP "cos");("sin",OP "sin" );("tan", OP "tan");( "log",OP "log");("plot", Plot);("def",Def)] |> dict |> Dictionary


    // Helper function to convert a string to a list of characters
    let str2lst s = [for c in s -> c]

    // Checks if a character is a whitespace
    let isblank c = System.Char.IsWhiteSpace c

    // Checks if a character is a digit
    let isdigit c = System.Char.IsDigit c

    // Checks if a character is a letter
    let isLetter c = System.Char.IsLetter c

    // Convert a character to its integer value (only for digits)
    let intVal (c:char) = (int)((int)c - (int)'0')

    // Recursively scans a numeric literal, including optional fractional parts
    let rec scNum(iStr, iVal:string) = 
        match iStr with
        | c :: tail when isdigit c -> scNum(tail, iVal + string c)
        | '.' :: tail -> 
            let (remainingInput, fraction) = scFraction tail ""
            (remainingInput, iVal + "." + fraction)
        | _ -> (iStr, iVal)

    // Handles the fractional part of a number
    and scFraction lst acc = 
        match lst with 
        | c :: tail when isdigit c -> scFraction tail (acc + string c)
        | _ -> (lst, acc)

    // Recursively scans strings (e.g., variable names, keywords)
    let rec scString(iStr, cVal : String) = 
        match iStr with
        | c :: tail when isLetter c || isdigit c -> scString(tail, cVal + string c)
        | _ -> (iStr, cVal)

    // Main lexical analyzer (lexer) to tokenize the input string
    let lexer input = 
        let rec scan input =
            match input with
            | [] -> [] // End of input
            | '+'::tail -> Add :: scan tail
            | '-'::tail -> Sub :: scan tail
            | '*'::tail -> Mul :: scan tail
            | '/'::tail -> Div :: scan tail
            | '%'::tail -> Mod :: scan tail
            | '^'::tail -> Pow :: scan tail
            | '('::tail -> Lpar :: scan tail
            | ')'::tail -> Rpar :: scan tail
            | '='::tail -> Equ :: scan tail
            | ','::tail -> Comma :: scan tail
            | c :: tail when isblank c -> scan tail // Skip whitespace
            | c :: tail when isdigit c -> 
                let (iStr, iVal) = scNum(tail, string c)
                if (iStr.Length > 0 && iStr.Head = 'E') then 
                    Num iVal :: E :: scan iStr.Tail 
                else
                    Num iVal :: scan iStr
            | c :: tail when isLetter c -> 
                let (iStr, cVal) = scString(tail, string c)
                if(lut.ContainsKey(cVal))then lut[cVal]::scan iStr else if(iStr.Length > 0 && iStr.Head = '(')then Func cVal :: scan iStr else Var cVal :: scan iStr
            | _ -> raise (System.Exception($"Lexer error: Invalid character '{input.Head}'"))
        scan (str2lst input)

    // Represents complex numbers (not fully used yet)
    type complex =
        val a: int // Real part
        val b: int // Imaginary part

    // Defines numeric types and their arithmetic operations
    type num = FLOAT of float | INT of int // Two types of numbers: float and int
               member this.GetValue = 
                   match this with
                   | FLOAT a -> a
                   | INT i -> float i // Convert int to float for consistency

               // Perform arithmetic between two numbers, handling type differences
               static member private performArithmetic op (a:num) (b:num) : num =
                   match a, b with 
                   | FLOAT x, FLOAT y -> FLOAT (op x y)
                   | FLOAT x, INT y -> FLOAT (op x (float y))
                   | INT x, FLOAT y -> FLOAT (op (float x) y)
                   | INT x, INT y -> INT ((int (op x y)))

               // Overloaded operators for arithmetic
               static member (+) (a:num, b:num) = num.performArithmetic (+) a b
               static member (-) (a:num, b:num) = num.performArithmetic (-) a b
               static member (*) (a:num, b:num) = num.performArithmetic (*) a b
               static member (%) (a:num, b:num) = num.performArithmetic (%) a b
               static member Pow (a:num, b:num) = 
                   let pow (x: float) (y: float) = x ** y
                   num.performArithmetic pow a b
               static member (/) (a:num, b:num) = 
                   match a, b with 
                   | INT x, INT y when y <> 0 -> FLOAT (float x / float y) 
                   | FLOAT x, FLOAT y when y <> 0.0 -> FLOAT (x / y) 
                   | FLOAT x, INT y when y <> 0 -> FLOAT (x / float y) 
                   | INT x, FLOAT y when y <> 0.0 -> FLOAT (float x / y) 
                   | _ -> failwith "Parser error: Division by 0"
        
    
    // Converts a general object to a numeric type (int or float)
    // If the type is unsupported, it throws an exception

    let toNum (value: obj) =
            match value with
            | :? int as i -> INT i
            | :? float as f -> FLOAT f 
            | _ -> failwith "Unsupported type"

    // Recursively extracts parameter names from the token list
    // Input: tList - list of terminals, parameters - list of collected parameters
    // Returns: Remaining token list and the extracted parameters
    
    let rec getParams (tList:terminal list) (parameters:terminal list) =
        match tList with
        | Var name :: tail -> getParams tail (List.append parameters [Var name])  // Add the variable name to the parameters list
        | Comma :: tail -> getParams tail parameters // Skip commas and continue
        | Rpar :: Equ :: tail -> (tail,parameters) // End of parameter list, return the remaining tokens and parameters
        |_-> failwith "Parser error: Invalid token in parameters" // Invalid token in the parameter list


    let rec setVarEqualToParam (tList:terminal list) (parameters:num list) (varTable:Dictionary<string,num>) = 
            match tList with
            | [] -> (0)
            | Var name :: tail when isdigit name[0]->if(int name > parameters.Length) then
                                                                        raise (System.Exception("Invalid number of parameters"))
                                                                      else varTable[name] <- parameters[int name]
                                                                           setVarEqualToParam tail parameters varTable
            | c :: tail-> setVarEqualToParam tail parameters varTable
           
    // BNF:
    //<E>        ::= <T> <Eopt> | "Var" <variable> "=" <E> | "Var" <name> "(" <parameters> ")" "="  <E>
    //<Eopt>     ::= "+" <T> <Eopt> | "-" <T> <Eopt> | <empty>
    //<T>        ::= <F> <Topt>
    //<Topt>     ::= "*" <F> <Topt> | "/" <F> <Topt> | "%" <F> <Topt> | <empty>
    //<F>        ::= <U> <Fopt>
    //<Fopt>     ::= "^" <U> <Fopt> | <empty>
    //<U>        ::= "-" <NR> | <NR>
    //<NR>       ::= "Num" <value> <NReopt> | "(" <E> ")" | "Var" <variable> | "OP" "(" <E> ")" | "Var" <name> "(" <arguments> ")"
    //<NReopt>   ::= "e" <Eexp> | <empty>
    //<Eexp>     ::= "-" <value> | <value> | "(" <E> ")"

    // Find derivative
    let deriv (funcName, funcTable:Dictionary<string, terminal list>) =
        let func = funcTable[funcName]
        func

    // The main function to parse and evaluate expressions
    let parseNeval (tList,varTable:Dictionary<string,num>,funcTable:Dictionary<string,terminal list>) = 
        // Recursive function for parsing expressions
        let rec E tList = 
            match tList with
            // Handle plot command: retrieve the function definition from funcTable
            | Plot :: Func name :: tail -> try
                                           (funcTable[name], toNum 1)
                                           with 
                                           // Function not found in funcTable
                                           | :? System.Collections.Generic.KeyNotFoundException -> raise (System.Exception($"Parser error: Function '{name}' not declared"))
            // Handle variable assignment                              
            | Var name :: Equ :: tail -> let (tList, value) = E tail // Parse the right-hand side expression
                                         varTable.[name] <- value   //Assign the value to the variable
                                         (tList, value)

            // Handle function definition
            | Def :: Func name :: Lpar :: tail ->let (tail,parameters) = getParams tail [] // Parse function parameters
                                                 funcTable.[name] <- (fun (listToModify:terminal list) (replacements:terminal list)  ->
                                                                     listToModify |> List.map (fun item -> if List.contains item replacements then Var (string (List.findIndex ((=) item) replacements)) else item)) tail parameters
                                                 ([], toNum 1.0) // Return success indicator
            // Handle other cases by delegating to T >> Eopt
            | _ -> (T >> Eopt) tList
                   
            // Parses optional addition or subtraction in expressions
        and Eopt (tList, value) = 
            match tList with
            | Add :: tail -> let (tLst, tval) = T tail
                             Eopt (tLst, value + tval)
            | Sub :: tail -> let (tLst, tval) = T tail
                             Eopt (tLst, value - tval)
            | Var name ::tail -> raise (System.Exception($"Parser error: Missing Operator"))
            | Lpar :: tail ->  raise (System.Exception($"Parser error: Missing Operator"))
            | _ -> (tList,value)
            // Parses terms, delegating to Topt for multiplication, division, etc.
        and T tList = (F >> Topt) tList
        and Topt (tList, value) =
            match tList with
            | Mul :: tail -> let (tLst, tval) = F tail
                             Topt (tLst,value * tval)
            | Div :: tail -> let (tLst, tval) = F tail
                             Topt (tLst,value / tval)
            | Mod :: tail -> let (tLst, tval) = F tail
                             Topt (tLst, value % tval)
            | _ -> (tList, value)
        // Parses factors, delegating to Fopt for exponentiation

        and F tList = (U >> Fopt) tList
        // Parses optional exponentiation operations
        and Fopt (tList,value) = 
                match tList with 
                | Pow :: tail -> let (tLst ,tval) = U tail 
                                 Fopt(tLst,value ** tval)
                | _ ->(tList,value)
        // Parses unary operators such as negation
        and U tList = 
            match tList with
            | Sub :: tail ->
                let (tLst,tVal:num) = NR tail
                (tLst,INT(0) - tVal)
            | _-> NR tList
        // Parses numeric values, variables, and function calls
        and NR tList =
            match tList with 
            // Built-in function calls
            | OP name :: Lpar :: tail -> let (tLst, tval) = E tail
                                         match tLst with 
                                         | Rpar :: tail -> (tail,(fun input -> match input with 
                                                                                      | "cos" -> FLOAT(Math.Cos(tval.GetValue))
                                                                                      | "tan" -> FLOAT(Math.Tan(tval.GetValue))
                                                                                      | "log" -> FLOAT(Math.Log(tval.GetValue))
                                                                                      | "sin" -> FLOAT(Math.Sin(tval.GetValue))
                                                                                      ) name)
                                         | _ -> raise (System.Exception($"Parser error: Incorrect use of built-in function '{name}'"))

            // User-defined function calls
            |Func name :: Lpar :: tail -> let (args, tail) = parseArguments tail []
                                          try
                                          let a = setVarEqualToParam funcTable.[name] args varTable
                                          let (etail,result) = E funcTable.[name]
                                          (tail, result)
                                          with 
                                          | :? System.Collections.Generic.KeyNotFoundException -> raise (System.Exception($"Parser error: Function '{name}' not declared"))     
            // Parsing numbers in scientific notation
            | Num value :: E :: exp -> let (tLst,eVal : num)  = Eexp exp
                                       (tLst, FLOAT((float)(value) * (10.0 ** (eVal.GetValue))))
            // Numbers used as variables (error case)
            | Num value :: Equ :: tail -> raise (System.Exception($"Parser error: Number '{value}' used as a variable name"))
            // Normal number parsing
            | Num value :: tail -> (tail, (if(value.Contains("."))then FLOAT(float value) else INT(int value)))
            // Handle parentheses in expressions
            | Lpar :: tail -> let (tLst, tval) = E tail
                              match tLst with 
                              | Rpar :: tail -> (tail, tval)
                              | _ -> raise (System.Exception("Parser error: Open bracket was not closed"))
            // Variable lookup
            | Var name :: tail -> try (tail, varTable.[name])
                                  with
                                    | :? System.Collections.Generic.KeyNotFoundException -> raise (System.Exception($"Parser error: Variable '{name}' not declared"))
            // Catch-all for invalid expressions
            | _ -> raise (System.Exception("Parser error: Invalid expression"))
            // Parses exponential expressions
        and Eexp tList = 
            match tList with
            | Sub :: Num value :: tail -> (tail,INT(-(int value)))
            | Num value :: tail -> (tail,INT(int value))
            | Lpar :: tail -> let (tLst, tval) = E tail
                              match tLst with 
                              | Rpar :: tail -> (tail, tval)
                              | _ -> raise (System.Exception("Parser error: Open bracket was not closed"))
            | _ -> raise (System.Exception("Parser error: Invalid expression"))


        // Used to find the pararmeter and so is not part of the BNF
        and parseArguments tList arguments =
           match tList with
           | Rpar :: tail -> (arguments,tail)
           | Comma :: tail -> parseArguments tail arguments
           | c :: tail ->let(tail, result) = U (c::tail)
                         parseArguments tail (List.append arguments [result])
           //| Num value :: Dot :: Num value2 :: tail -> parseArguments tail (List.append arguments [FLOAT((float)((string) value + "." + (string)value2))])
           //| Num value :: tail -> parseArguments tail (List.append arguments [INT(value)])
           //| Var name :: tail -> parseArguments tail (List.append arguments [varTable[name]])
           | _ -> raise (System.Exception("Parser error: Invalid argument list"))
        E tList

    (*//Check without evaluation
    let parser tList = 
        let rec E tList = (T >> Eopt) tList         // >> is forward function composition operator: let inline (>>) Eopt T tList = Eopt(T(tList))
        and Eopt tList = 
            match tList with
            | Add :: tail -> (T >> Eopt) tail
            | Sub :: tail -> (T >> Eopt) tail
            | _ -> tList
        and T tList = (NR >> Topt) tList
        and Topt tList =
            match tList with
            | Mul :: tail -> (NR >> Topt) tail
            | Div :: tail -> (NR >> Topt) tail
            | Mod:: tail -> (NR >> Topt) tail
            | Pow :: tail -> (NR >> Topt) tail
            | _ -> tList
        and NR tList =
            match tList with 
            | Num value :: tail -> tail
            | Lpar :: tail -> match E tail with 
                              | Rpar :: tail -> tail
                              | _ -> raise parseError
            | _ -> raise parseError
        E tList*)


// This is to intialize the LUT.
    [<EntryPoint>]
        let main _ = 0