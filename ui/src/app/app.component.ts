import { Component, ViewChild, ElementRef, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { CompileService } from './compileService';
import { OuterLexemes, LexemInCode, Identifier, Constant, LexicalError, Grammar, Result } from './models/Lexems';
import { MatTableDataSource } from '@angular/material/table';
//import { CompileService } from '.';
@Component({
	selector: 'app-root',
	templateUrl: './app.component.html',
	styleUrls: ['./app.component.sass'],
	providers: [CompileService]
})


export class AppComponent implements OnInit {
	fromFile: boolean = true;
	codetext: string;
	codefile: any;
	form: FormGroup;
	outerLexemes: OuterLexemes = new OuterLexemes();
	dataSourceLexems = new MatTableDataSource<LexemInCode>();
	displayedColumns: string[] = ['lineNumber', 'name', 'token', 'index'];

	dataSourceId = new MatTableDataSource<Identifier>();
	displayedColumnsId: string[] = ['name', 'type', 'index'];

	dataSourceCon = new MatTableDataSource<Constant>();
	displayedColumnsCon: string[] = ['name', 'index'];

	dataSourceErr = new MatTableDataSource<LexicalError>();
	displayedColumnsErr: string[] = ['line', 'text'];

	dataSourceGr = new MatTableDataSource<Grammar>();
	displayedColumnsGr: string[] = ['token', 'text'];

	dataSourceSyntax = new MatTableDataSource<LexicalError>();
	displayedColumnsErrSyn: string[] = ['line', 'text'];

	@ViewChild('fileInput') fileInput: ElementRef;

	PrintToken(token: string): string
	{
		if (token === "\n")
		{
			return "¶";
		}
		else 
		{
			return token;
		}
	}


	PrintLexeme(token: string): string
	{
		if (token === "delimiter")
		{
			return "¶";
		}
		else 
		{
			return token;
		}
	}


	ngOnInit() {

	}

	constructor(private fb: FormBuilder, private compileService: CompileService) {
		this.createForm();
	}

	createForm() {
		this.form = this.fb.group({
			file: null
		});
	}

	onFileChange(event) {
		let reader = new FileReader();
		if (event.target.files && event.target.files.length > 0) {
			let file = event.target.files[0];
			reader.readAsDataURL(file);
			reader.onload = () => {
				this.form.get('file').setValue({
					filename: file.name,
					filetype: file.type,
					value: (reader.result as string).split(",")[1]
				})
			};
		}
	}

	onCompileTextClick(): void {
		this.dataSourceLexems.data = null;
		this.dataSourceId.data = null;
		this.dataSourceCon.data = null;
		this.dataSourceErr.data = null;
		this.dataSourceGr.data = null;
		this.dataSourceSyntax.data=null;
		this.compileService.postString(this.codetext).subscribe((data: Result) => {
			this.outerLexemes = data.outerLexemes;
			if (data !== null && data.outerLexemes != null) {
				this.dataSourceLexems.data = data.outerLexemes.lexems;
				this.dataSourceId.data = data.outerLexemes.identifiers;
				this.dataSourceCon.data = data.outerLexemes.constants;
				this.dataSourceErr.data = data.outerLexemes.errors;
				this.dataSourceGr.data = data.outerLexemes.grammar;
			}
			if (data !== null && data.syntaxResult != null) {
				this.dataSourceSyntax.data = data.syntaxResult.text;
			}
		});
	}

	onCompileFileClick(): void {
		this.dataSourceLexems.data = null;

		this.dataSourceSyntax.data=null;
		this.dataSourceId.data = null;
		this.dataSourceCon.data = null;
		this.dataSourceErr.data = null;

		this.compileService.postString(atob(this.form.value.file.value)).subscribe((data: Result) => {
			this.outerLexemes = data.outerLexemes;
			if (data !== null && data.outerLexemes != null) {
				this.dataSourceLexems.data = data.outerLexemes.lexems;
				this.dataSourceId.data = data.outerLexemes.identifiers;
				this.dataSourceCon.data = data.outerLexemes.constants;
				this.dataSourceErr.data = data.outerLexemes.errors;
				this.dataSourceGr.data = data.outerLexemes.grammar;
			}
			if (data !== null && data.syntaxResult != null) {
				this.dataSourceSyntax.data = data.syntaxResult.text;
			}
		});;
	}
}
