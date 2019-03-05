# oss
[download here](https://www.aspnetawesome.com/Download/Oss) (samples included)

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
    border-witdh: 1px;  
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
    border-witdh: 1px;  
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
sample4base.css:
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
```
