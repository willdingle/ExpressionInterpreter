// Simple Interpreter in F#
// Author: R.J. Lapeer 
// Date: 23/10/2022
// Reference: Peter Sestoft, Grammars and parsing with F#, Tech. Report

namespace FsharpLib
open System
open System.Collections.Generic

module Interpreter = 

    type terminal = 
        Add | Sub | Mul | Div | Lpar | Rpar | Mod | Pow | Var of String | Equ | Dot| Num of int | E

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
            | c :: tail when isblank c -> scan tail
            | c :: tail when isdigit c -> let (iStr, iVal) = scInt(tail, intVal c)
                                          if(iStr.Length > 0 && (iStr.Head = 'E' || iStr.Head = 'e')) then Num iVal :: E :: scan iStr.Tail else Num iVal :: scan iStr
            | c :: tail when isLetter c -> let (iStr, cVal) = scString(tail, string c)
                                           Var cVal :: scan iStr
            | _ -> raise (System.Exception("Lexer error: Invalid character"))
        scan (str2lst input)

    let getInputString() : string = 
        Console.Write("Enter an expression: ")
        Console.ReadLine()

   // BNF:
    //<E>        ::= <T> <Eopt> | "Var" <variable> "=" <E>
    //<Eopt>     ::= "+" <T> <Eopt> | "-" <T> <Eopt> | <empty>
    //<T>        ::= <F> <Topt>
    //<Topt>     ::= "*" <F> <Topt> | "/" <F> <Topt> | "%" <F> <Topt> | <empty>
    //<F>        ::= <U> <Fopt>
    //<Fopt>     ::= "^" <U> <Fopt> | <empty>
    //<U>        ::= "-" <NR> | <NR> 
    //<NR>       ::= "Num" <value> <NReopt> | "(" <E> ")" | "Var" <variable>
    //<NReopt>   ::= "e" <Eexp> | <empty>
    //<Eexp>     ::= "+" <value> | "-" <value> | <value>


    //Check with evaluation
    let parseNeval (tList,varTable:Dictionary<string,double>) = 
        let rec E tList = 
            match tList with
            | Var name :: Equ :: tail -> let (tList, value) = E tail
                                         varTable.[name] <- value
                                         (tList, value)
            | _-> (T >> Eopt) tList
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
                             Topt (tLst, value * tval)
            | Div :: tail -> let (tLst, tval) = F tail
                             if not (tval = 0.0) then Topt (tLst, value / tval) else raise (System.Exception("Parser error: Division by 0"))
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
                let (tLst,tVal) = NR tail 
                (tLst,-tVal)
            | _-> NR tList
        and NR tList =
            match tList with 
            | Num value :: Dot :: Num value2 :: tail -> (tail, (float)((string) value + "." + (string)value2))
            | Num value :: E :: exp -> let (tLst,eVal)  = Eexp exp
                                       (tLst,(float)value * (10.0 ** eVal))
            | Sub :: Num value :: tail -> (tail, -value)
            | Num value :: Equ :: tail -> raise (System.Exception("Parser error: Number used as a variable name"))
            | Num value :: tail -> (tail, value)
            | Lpar :: tail -> let (tLst, tval) = E tail
                              match tLst with 
                              | Rpar :: tail -> (tail, tval)
                              | _ -> raise (System.Exception("Parser error: Open bracket was not closed"))
            | Sub :: Var name:: tail -> (tail, -varTable.[name])
            | Var name:: tail -> try (tail, varTable.[name])
                                 with
                                    | :? System.Collections.Generic.KeyNotFoundException -> raise (System.Exception("Parser error: Variable not declared"))
            | _ -> raise (System.Exception("Parser error: Invalid expression"))
        and Eexp tList = 
            match tList with
            | Add :: Num value :: tail -> (tail,value)
            | Sub :: Num value :: tail -> (tail,-value)
            | Num value :: tail -> (tail,value)
            | _ -> raise (System.Exception("Parser error: Invalid expression"))
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

    let rec printTList (lst:list<terminal>) : list<string> = 
        match lst with
        head::tail -> Console.Write("{0} ",head.ToString())
                      printTList tail
                  
        | [] -> Console.Write("EOL\n")
                []