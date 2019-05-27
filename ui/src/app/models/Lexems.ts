
export class Result {
    outerLexemes: OuterLexemes;
    syntaxResult:SyntaxResult;
}


export class OuterLexemes {

    lexems: LexemInCode[];
    constants: Constant[];
    identifiers: Identifier[];
    errors: LexicalError[];
    grammar: Grammar[];
}

export class Grammar {
    token: string;
    lexemes: string[];
}

export class LexemInCode {
    lineNumber: number;
    subString: string;
    token: string;
    index: string;
}

export class Constant {

    Name: string;
    Index: number;
}

export class Identifier {

    Name: string;
    Type: string;
    Index: number;
}

export class LexicalError {

    Line: number;
    Text: string;
}

export class SyntaxResult {

    Success: boolean;
    text: LexicalError[];
}