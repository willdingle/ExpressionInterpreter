// Simple Interpreter in F#
// Authors: Arthur Coombes, Will Dingle, Yucheng Bian
// Date: 04/11/2024
// Reference: Peter Sestoft, Grammars and parsing with F#, Tech. Report

namespace FsharpLib
open System
open System.Collections.Generic

module Interpreter = 

    //Use this later for the variables
    
    type terminal = 
        Add | Sub | Mul | Div | Lpar | Rpar | Mod | Pow | Var of String | Equ | Dot | Num of int | E | OP of string | Def | Plot | Comma

    let str2lst s = [for c in s -> c]
    let isblank c = System.Char.IsWhiteSpace c
    let isdigit c = System.Char.IsDigit c
    let isLetter c = System.Char.IsLetter c
    let intVal (c:char) = (int)((int)c - (int)'0')

    let rec scInt(iStr, iVal) = 
        match iStr with
        | c :: tail when isdigit c -> scInt(tail, 10*iVal+(intVal c))
        | _ -> (iStr, iVal)

    let rec scString(iStr, cVal : String) = 
        match iStr with
        c :: tail when isLetter c || isdigit c -> scString(tail, cVal + string c)
        | _ -> (iStr, cVal)


    let lexer input = 
        let rec scan input =
            match input with
            | [] -> []
            | '+'::tail -> Add :: scan tail
            | '-'::tail -> Sub :: scan tail
            | '*'::tail -> Mul :: scan tail
            | '/'::tail -> Div :: scan tail
            | '%'::tail -> Mod:: scan tail
            | '^'::tail -> Pow:: scan tail
            | '('::tail -> Lpar:: scan tail
            | ')'::tail -> Rpar:: scan tail
            | '='::tail -> Equ :: scan tail
            | '.'::tail -> Dot :: scan tail
            | ','::tail -> Comma :: scan tail
            | 'd' :: 'e' :: 'f' :: tail -> Def :: scan tail
            | c :: tail when isblank c -> scan tail
            | c :: tail when isdigit c -> let (iStr, iVal) = scInt(tail, intVal c)
                                          if(iStr.Length > 0 && iStr.Head = 'E') then Num iVal :: E :: scan iStr.Tail else Num iVal :: scan iStr
            | c :: tail when isLetter c -> let (iStr, cVal) = scString(tail, string c)
                                           let checkinput input = 
                                               match input with
                                                   | "cos" -> OP "cos" :: scan iStr
                                                   | "tan" -> OP "tan" :: scan iStr
                                                   | "sin" -> OP "sin" :: scan iStr
                                                   | "log" -> OP "log" :: scan iStr
                                                   //| "plot" -> Plot :: scan iStr
                                                   | _ -> Var cVal :: scan iStr
                                           checkinput cVal
            | _ -> raise (System.Exception("Lexer error: Invalid character"))
        scan (str2lst input)

    // a: real part
    // b: imaginary part
    type complex =
        val a: int
        val b: int

    // Overloaded the operator for the num values
    // Because the operators are functions we can call them like (operator) a b. 
    // So for "(+) 1 1" would return 1 + 1 = 2
    // Here we are calling performArithmetic and passing the function op and op takes a,b as parameters
    // performArithmetic checks a and b types and returns the type that matches.
    // So INT + INT -> INT

    type num = FLOAT of float | INT of int //| COMPLEX of complex
               member this.GetValue = match this with
                                        | FLOAT a -> a
                                        | INT i -> int i

               static member private performArithmetic op (a:num) (b:num) : num =
                      match a, b with 
                      | FLOAT x, FLOAT y -> FLOAT (op x y)
                      | FLOAT x, INT y -> FLOAT (op x (float y))
                      | INT x, FLOAT y -> FLOAT (op (float x) y)
                      | INT x, INT y -> INT ((int (op x y)))

               static member (+) (a:num, b:num) = num.performArithmetic (+) a b
               static member (-) (a:num, b:num) = num.performArithmetic (-) a b
               static member (*) (a:num, b:num) = num.performArithmetic (*) a b
               static member (%) (a:num, b:num) = num.performArithmetic (%) a b
               static member Pow (a:num, b:num) = let pow (x: float) (y: float) = x ** y
                                                  num.performArithmetic pow a b
               static member (/) (a:num, b:num) = match a, b with 
                                                  | INT x, INT y when y <> 0 -> FLOAT (float x / float y) 
                                                  | FLOAT x, FLOAT y when y <> 0.0 -> FLOAT (x / y) 
                                                  | FLOAT x, INT y when y <> 0 -> FLOAT (x / float y) 
                                                  | INT x, FLOAT y when y <> 0.0 -> FLOAT (float x / y) 
                                                  | _ -> failwith "Parser error: Division by 0"
        
    let toNum (value: obj) =
            match value with
            | :? int as i -> INT i
            | :? float as f -> FLOAT f 
            | _ -> failwith "Unsupported type"

    let rec getParams (tList:terminal list) (parameters:terminal list) =
        match tList with
        | Var name :: tail -> getParams tail (List.append parameters [Var name])
        | Comma :: tail -> getParams tail parameters
        | Rpar :: Equ :: tail -> (tail,parameters)
        |_-> failwith "Parser error: Invalid token in parameters"


    let rec setVarEqualToParam (tList:terminal list) (parameters:num list) (varTable:Dictionary<string,num>) = 
            match tList with
            | [] -> (0)
            | Var name :: tail when name |> Seq.forall Char.IsDigit->if(int name > parameters.Length) then
                                                                        raise (System.Exception("Invalid number of parameters"))
                                                                      else varTable[name] <- parameters[int name]
                                                                           setVarEqualToParam tail parameters varTable
            | c :: tail-> setVarEqualToParam tail parameters varTable
           
    // BNF:
    //<E>        ::= <T> <Eopt> | "Var" <variable> "=" <E> | "Func" <name> "(" <variable> ")" "="  <E>
    //<Eopt>     ::= "+" <T> <Eopt> | "-" <T> <Eopt> | <empty>
    //<T>        ::= <F> <Topt>
    //<Topt>     ::= "*" <F> <Topt> | "/" <F> <Topt> | "%" <F> <Topt> | <empty>
    //<F>        ::= <U> <Fopt>
    //<Fopt>     ::= "^" <U> <Fopt> | <empty>
    //<U>        ::= "-" <NR> | <NR> | "Func" "(x)"
    //<NR>       ::= "Num" <value> <NReopt> | "(" <E> ")" | "Var" <variable> | "OP" "(" <E> ")"
    //<NReopt>   ::= "e" <Eexp> | <empty>
    //<Eexp>     ::= "-" <value> | <value> | "(" <E> ")"


    //Check with evaluation
    let parseNeval (tList,varTable:Dictionary<string,num>,funcTable:Dictionary<string,terminal list>) = 
        let rec E tList = 
            match tList with
            | Var name :: Equ :: tail -> let (tList, value) = E tail
                                         varTable.[name] <- value
                                         (tList, value)

            //| Def :: Var name :: Lpar :: params :: Rpar :: Equ :: tail ->funcTable.[name] <- tail
             //                                                            let a = params
              //                                                           ([], toNum 1.0)


            | Def :: Var name :: Lpar :: tail ->let (tail,parameters) = getParams tail []
                                                funcTable.[name] <- (fun (listToModify:terminal list) (replacements:terminal list)  ->
                                                                     listToModify |> List.map (fun item -> if List.contains item replacements then Var (string (List.findIndex ((=) item) replacements)) else item)) tail parameters
                                                ([], toNum 1.0)

            | _ -> (T >> Eopt) tList
        and Eopt (tList, value) = 
            match tList with
            | Add :: tail -> let (tLst, tval) = T tail
                             Eopt (tLst, value + tval)
            | Sub :: tail -> let (tLst, tval) = T tail
                             Eopt (tLst, value - tval)
            | _ -> (tList, value)
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
        and F tList = (U >> Fopt) tList
        and Fopt (tList,value) = 
                match tList with 
                | Pow :: tail -> let (tLst ,tval) = U tail 
                                 Fopt(tLst,value ** tval)
                | _ ->(tList,value)
        and U tList = 
            match tList with
            | Sub :: tail ->
                let (tLst,tVal:num) = NR tail
                (tLst,INT(0) - tVal)
            | _-> NR tList
        and NR tList =
            match tList with 
            | OP name :: Lpar :: tail -> let (tLst, tval) = E tail
                                         match tLst with 
                                         | Rpar :: tail -> (tail,(fun input -> match input with 
                                                                                      | "cos" -> FLOAT(Math.Cos(tval.GetValue))
                                                                                      | "tan" -> FLOAT(Math.Tan(tval.GetValue))
                                                                                      | "log" -> FLOAT(Math.Log(tval.GetValue))
                                                                                      | "sin" -> FLOAT(Math.Sin(tval.GetValue))
                                                                                      ) name)
                                         | _ -> raise (System.Exception("Parser error: Incorrect use of built-in function"))


            | Var name :: Lpar :: tail -> let (args, tail) = parseArguments tail []
                                          try
                                          let a = setVarEqualToParam funcTable.[name] args varTable
                                          let (etail,result) = E funcTable.[name]
                                          (tail, result)
                                          with 
                                          | :? System.Collections.Generic.KeyNotFoundException -> raise (System.Exception("Parser error: Function not declared"))     


            | Num value :: Dot :: Num value2 :: tail -> (tail, FLOAT((float)((string) value + "." + (string)value2)))
            | Num value :: E :: exp -> let (tLst,eVal : num)  = Eexp exp
                                       (tLst, FLOAT((float)(value) * (10.0 ** (eVal.GetValue))))
            | Num value :: Equ :: tail -> raise (System.Exception("Parser error: Number used as a variable name"))
            | Num value :: tail -> (tail, INT(value))
            | Lpar :: tail -> let (tLst, tval) = E tail
                              match tLst with 
                              | Rpar :: tail -> (tail, tval)
                              | _ -> raise (System.Exception("Parser error: Open bracket was not closed"))
            | Var name :: tail -> try (tail, varTable.[name])
                                  with
                                    | :? System.Collections.Generic.KeyNotFoundException -> raise (System.Exception("Parser error: Variable not declared"))
            | _ -> raise (System.Exception("Parser error: Invalid expression"))
        and Eexp tList = 
            match tList with
            | Sub :: Num value :: tail -> (tail,INT(-value))
            | Num value :: tail -> (tail,INT(value))
            | Lpar :: tail -> let (tLst, tval) = E tail
                              match tLst with 
                              | Rpar :: tail -> (tail, tval)
                              | _ -> raise (System.Exception("Parser error: Open bracket was not closed"))
            | _ -> raise (System.Exception("Parser error: Invalid expression"))


        //Used to find the pararmeter and so is not part of the BNF
        and parseArguments tList arguments =
           match tList with
           | Rpar :: tail -> (arguments,tail)
           | Num value :: tail -> parseArguments tail (List.append arguments [INT(value)])
           | Var name :: tail -> parseArguments tail (List.append arguments [varTable[name]])
           | Comma :: tail -> parseArguments tail arguments
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