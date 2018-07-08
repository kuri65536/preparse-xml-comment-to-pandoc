platform := $(shell uname)
ifeq ($(platform),Linux)
    prepandoc := ./prepandoc.exe
    pandoc_path := 
    pandoc_tmps := 
    pandoc := $(pandoc_path)pandoc
    output := result.html
    output1 := sample-this-proj.md
    launch := browse
else  # for windows (msys).
    prepandoc := tools/prepandoc.exe
    pandoc_path := # /c/app/Pandoc/
    pandoc_tmps := # /c/Users/who/Desktop/scripts/pandoc-template
    pandoc := $(pandoc_path)pandoc.exe
    output := result.docx
    launch := start
endif

CS_OPTIONS := -r:System.Windows.Forms.dll -r:System.Drawing.dll \
              -debug
# CS_OPTIONS += -r:System.Collections.Generic.dll

bin := prepandoc.exe
src := prepandoc.cs config.cs common.cs

all: doc

build: $(bin)

$(bin): $(src)
	mcs $(CS_OPTIONS) -out:$@ $^


FC := $(pandoc_tmps)/template/template_meiryo.docx
FB := $(pandoc_tmps)/config/crossref_config.yaml
FT := $(pandoc_tmps)/config/bibliography.bib

path_doc := .

opts_pandoc := \
               --toc --template=template1.html
#              -F pandoc-crossref \
#              -M crossrefYaml="$(FC)" \
#              --bibliography "$(FB)" \
#              --reference-doc="$(FT)" \

doc: $(bin)
	# export PATH="$(PATH):$(pandoc_path)";
	$(prepandoc) $(path_doc) $(path_doc)/README.md temp.md
	$(pandoc) $(opts_pandoc1) -o $(output1) temp.md
	$(pandoc) $(opts_pandoc) -o $(output) $(output1)
	$(launch) $(output)


include Makefile.test

# vi: ft=make:et:ts=4:fdm=marker
