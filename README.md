Umbraco Enhanced Markdown Property Editor
============

# Status

This project is in early stages of development and is not production ready

If you would like to collaborate please get in contact we need contributions!

# Why is this a thing, Umbraco has a Markdown editor already?

Unlike the native Umbraco editor the Enhanced Markdown editor includes support for local link parsing so that URL changes are rendered correctly, support for anchor targets and also support for title attributes to be rendered on the anchor tags

![](https://raw.githubusercontent.com/CrumpledDog/Umbraco-EnhancedMarkdown/develop/docs/images/example.png)

# Issues/things to do

- Handle UDIs?
- Implement Property Value Converter to that output is IHtmlString
- Handle MediaIds so that they are resolved at render time
- Support GFM
- Utilise MarkDownSharp instead of Markdown Deep as it's already included in Umbraco now?
- Doesn't like it when there is a / in the title

# Test Site & Source Code

A test site is included in the solution, the username and password for Umbraco are admin@admin.com/password12345
By default the test site is configured to use full IIS (due to IIS Express SQL CE persistence issue) on the domain enhancedmarkdown.local, you can change it to use IIS Express if you prefer.

Visual Studio 2017 is required for compiling the source code

# Credits and references

This project may include code that originated in the UmbracoCms project, Umbraco Ditto project & other OSS MIT licensed projects