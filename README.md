# CSV Parser
Csv parser is a lightweight and flexible library that permets you to map data from csv tables to your c# records in order to process them and store/send them somwhere else.

## Get started
A <code>CsvParser\<T\></code> is instanciated to serve only to one type of entity (and one type of csv document). In order to get a working parser, you must always call the method at <code>Build()</code>:


> `var parser = new CsvParser<T>().Build();`


Note that parser is looking only at your entitie's properies (your table can have additional data, your record - can't). In order to parse an entity from a stream call `ParseStream(Stream stream)`method:

> `IList<T> csvData = parser.ParseStream(stream);`

What it's going to do is: find columns that correspond `T`'s properties in the header, read stream and transform each row into an entity.
All the parse methods (there are overloads the takes `StreamReader` or file path as an argument) return `IList<T>` as result.
Note that all of them except `CsvReader<T>.Parse(StreamReader reader)` would automatically close the stream with csv data.
>
Csv parser automatically resolves how to convert csv data. Out of box it can resolve all culture-invariant number writings, strings, most of Date&Time types: `DateTime`, `TimeSpan` and `DateOnly`, it can deal quite good with `enum` types too (but that might demand additional settings)

## Parametrise your parser
So what are the parameters. Let's start with the most usefull ones.
### Writing Id's
If you're exposing an api to send data from csv documents to a database, it's likely that you have an `Id` property on your entity while those csv files you process would generally omit this information. Csv parser knows how to solve this problem:

>`var parser = new CsvParser<T>()`
><br>&nbsp;`.WriteId()`
><br>&nbsp;`.Build()`

As all of parser's options, this method is called during parser creation, before `Build()` invokation.
What this method does is it tells parser not to find Id column in the csv table, but to generate one itself. Parser suppots integer auto-augmenting or Guid indexes.
>Note that the index propetie <b>must</b> be called `Id`

>

>Pay attention using Id writing with explicit column declaration
### Finding corresponding columns
By default, parser would try to find the columns with the name exactly matching to its of a property. But generally, this won't be a case, as C# standart is to use PascalCase, which is quite unpopular beyond its ecosystem :(<br/>
With CsvParser you can easily specify the rules to transform PascalCased C# property names into csv column names. Let's see how:
>`var parser = new CsvParser<CoffeSaleEntity>()`<br>
&nbsp;&nbsp;&nbsp;`.WithHeaderParser(headerParser =>`<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`headerParser.AddNamingPolicy(NameParsingStandarts.PascalToSnakeCase))`<br>
&nbsp;&nbsp;&nbsp;`.Build();`

This method constucts an array of functions that parser would try to use to find each property a corresponding column in a csv file.
`NameParser` class exposes two public methods: `WithNamingProperty` and `AddNameingProperty`. The first is erasing all the functions, that were present on this `NameParser` object before. So for example
> `nameParser.AddNamingPolicy(a).WithNamingPolicy(b)`

results in `nameParser` having remembered only `b` policy.<br/>
To simplify things, I have introduces NameParsingStandarts, which you can use to add policies transforming PascalCase to snake_case (& vice-versa) or supressing spaces. However, if your workflow releves something more challenging, both methods has overloads accepting `Func<string, string>` as an argument, so you can add your own name transformers.
>Note that `NameParserStandarts.ExactMatch` policy is included by default in every `NameParser` object. 

### Finding corresponding columns (differently)
An alternative to the previous method is to declare, which column would serve for each property explicitely:
>`parser = new CsvParser<CoffeSaleEntity>()`<br>&nbsp;&nbsp;&nbsp;&nbsp;
`.WriteId()`<br>&nbsp;&nbsp;&nbsp;&nbsp;
`.WithExplicitColumnDeclaration(`<br>&nbsp;&nbsp;&nbsp;&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;`["coffee_type",`<br>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;`"Money",`<br>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;`"Date",`<br>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;`"Cash Type"])`
    `.Build();`

This method makes a map between a property (in the order of their appearence) and a column name, where the data for this property is stored. For example the previous could transform csv row to an entity like:
> `record CoffeSaleEntity(int Id, string CoffeType, decimal Price,`<br>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`DateOnly Date, PaymentType CashType)`

Pay attention to using this method with `WriteId()`. Declaring the columns explicitely you mustn't omit a single property of the parsing type, while `WriteId()` pops one out of the list to process it separately. So, when writing ids, make sure to make call to `WithExplicit ColumnDeclaration(...)` __after__ the call to `WriteId()`<br/>
There's also an overload of `WithExplicit ColumnDeclaration` that takes `int[]` array of column indexes as argument. The rule of `WriteId()` rests the same.

### Format
If you want, it's always possible to precise the format of data, to parse it more accurately. For now this feature is possible only for Date & Time types, but we would work in this direction in the future versions.
> `parser = new CsvParser<T>()`<br>&nbsp;&nbsp;&nbsp;&nbsp;
`.WithFormat(typeof(DateOnly), "yyyy-mm-dd")`<br>&nbsp;&nbsp;&nbsp;&nbsp;
> `.Build();`
### Enums
You can easely parse enum types with csv parser. The transformation of csv cell text into enumeration contant's name is also held by a name parser:
> `parser = new CsvParser<T>()`<br>&nbsp;&nbsp;&nbsp;&nbsp;
`.WithEnumParser(parser =>`<br>&nbsp;&nbsp;&nbsp;&nbsp;
&nbsp;&nbsp;&nbsp;&nbsp;`parser.WithNamingPolicy(`<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`NameParsingStandarts.SnakeCaseToPascal))`<br>&nbsp;&nbsp;&nbsp;&nbsp;
> `.Build();`

Note that this name parser would not transform C# PascalCase names into these of csv, but vice-versa.<br>Another factor to consider, is that you can have only one enum parser on the `CsvParser<T>` object. So if for exmple have to different columns, one of which has values like
> `card | cash | check`

And another column with values:
> `Milk Latte | Maccaciato | Double Expresso`

The you should add __both__ policies to supress spaces and to transform camel case to pascal on your enum parser.

## Exceptions
-  `CSVParserBuildingException` - thrown when something goes wrong at the moment of configuring the parser. This can be absence of Id property, when calling to `WriteId()`, or wrong array length of `WithExplicitColumnDeclaration(...)`.
-  `CsvParsingException` - this type of exception signals an error at the stage of preparing to process a csv file. Generally you see this exception when CsvParser can't find a column, corresponding to a property.
-  `ConvertException` - as the name says, this is an error of parsing string csv data to a specified type. Try to use `.WithFormat()` to tell the parser how to process this type correctly
