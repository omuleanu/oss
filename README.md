[download here](https://www.aspnetawesome.com/Download/Oss) (samples included)

to parse 1 file call:
```
dotnet Oss.dll parse sample1.css res1.css
```
after you run `parse` it will watch `sample1.css` for changes and save to `res1.css` every time you change `sample1.css` and you hit enter (close the console) if you want to stop watching for changes, same applies to the rest of parse commands,

use `-nowatch` if you don't want to watch for changes.

to parse multiple files: 
```
dotnet Oss.dll parse input1.css:res1.css input2.css:res2.css
```
parse multiple files listed in txt file:
```
dotnet Oss.dll parse -file all.txt
```
and `all.txt` could look like this:
```
sample1.css:res1.css
sample2.css:res2.css
```

## Variables

```
var x = red;
var y = 1;
var pad = 1.5em;
var z = @calc(2 + var.y)px;

var bl1 = { 
    font-size: 1em;
    border: var(y)px solid var.x;
};

.rule1
{
    color: var.x;
    border-width: var.z;
    var.bl1;
    padding: var.pad;
}
```
will output:
```
.rule1
{
    color: red;
    border-width: 3px;
    font-size: 1em;
    border: 1px solid red;
    padding: 1.5em;
}
```
## Inheritance
```
@name base1;
.rule1
{
    border-width: 1px;  
}

.rule2
{
    @inherit base1;
    color: blue;
}

.rule3
{
    @inherit base1;
}
```
will output:
```
.rule1,
.rule2,
.rule3
{
    border-width: 1px;  
}

.rule2
{
    color: blue;
}
```
## Insert file
```
var cl1 = green;
var cl2 = blue;

var a = 2;
var x = @calc(var.a * 1.5)em;

@insert sample4base.css;
```
sample4base.css content:
```
.rule1 {
    color: var.cl1;
    border-color: var.cl2;
    font-size: var.x;
}
```
will output:
```
.rule1 {
    color: green;
    border-color: blue;
    font-size: 3em;
}
```

## Variable with parameters
```
var color = green;

var bl1 = { 
    text-align: center;
    color: var.color;
};

.ruleWithParam
{
    border-radius: 5px;
    var(bl1){ color = red; };
}
```
will output:
```
.ruleWithParam
{
    border-radius: 5px;
    text-align: center;
    color: red;
}
```
## Rules Variables
A variable can contain whole css rules. These variables will be called using `@var.name`, it works the same as `@insert` except instead of content of file, the content of a variable is inserted. Parameters can't be set to these variables.
```
var zoom = 1.3;
var color = green;

var bl1 = { 
    text-align: center;
    color: var.color;
};

var rls1 = 
{   
    .cl1
    {
        zoom: var.zoom;
    }

    .cl2 
    {
        color: green;
        var.bl1;
    }
};

@var.rls1;
```
will output:
```
.cl1
{
    zoom: 1.3;
}

.cl2 
{
    color: green;
    text-align: center;
    color: green;
}
```
## Remove Empty Rule
If a css rule contains a call to a variable that has the value `rem` and nothing else, the rule will be removed.
```
var delempt = rem;
.willberemoved
{
    var.delempt;
}
```
will output nothing

## Lazy variables
To make a variable lazy add `:` in front of the name.
If you define a variable that calls other variables which are not defined yet, or intended to be only parameters, you will get an error when the current variable declaration is parsed, to avoid that you can make it lazy and the variable content will be parsed when it will be used.
```
var :bl1 = { 
    color: var.color;
};

.ruleWithParam
{
    var(bl1){ color = red; };
}
```
## Merge
You can merge the contents of 2 files by calling `merge`, example:
```
dotnet Oss.dll merge theme1.css theme2.css
```
so if theme1.css is:
```
var a = red;
var b = blue;
```
and theme2.css is:
```
var b = pink;
```
theme2.css will change to:
```
var a = red;
var b = pink;
```
theme2.css.bak will be created with the original content
