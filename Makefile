platform := $(shell uname)
ifeq ($(platform),Linux)
    prepandoc := ./prepandoc.exe
    pandoc_path := 
    pandoc_tmps := 
    pandoc := $(pandoc_path)pandoc
    output := result.html
    launch := browse
else  # for windows (msys).
    prepandoc := tools/prepandoc.exe
    pandoc_path := # /c/app/Pandoc/
    pandoc_tmps := # /c/Users/who/Desktop/scripts/pandoc-template
    pandoc := $(pandoc_path)pandoc.exe
    output := result.docx
    launch := start
endif
output1 := README.md

path_opt:=./Mono.Options.5.3.0.1/lib/net4-client

CS_OPTIONS := -r:System.Windows.Forms.dll -r:System.Drawing.dll \
              -r:$(path_opt)/Mono.Options.dll \
              -debug
              # -r:./Mono.Options.5.3.0.1/lib/netstandard1.3/Mono.Options.dll \
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

opts_pandoc1 := \
               --toc -V toc-title:"table of contents" \
               --include-after-body=CHANGE.log \
               --template=template1.tmpl
#              -F pandoc-crossref \
#              -M crossrefYaml="$(FC)" \
#              --bibliography "$(FB)" \
#              --reference-doc="$(FT)" \

doc: export MONO_PATH :=$(path_opt)
doc: $(bin)
	# export PATH="$(PATH):$(pandoc_path)";
	$(prepandoc) $(path_doc) $(path_doc)/source.md temp.md
	$(pandoc) $(opts_pandoc1) -o $(output1) temp.md
	$(pandoc) $(opts_pandoc) -o $(output) $(output1)
	$(launch) $(output)

ref: $(bin)
	doxygen Doxyfile

deploy: ver:=$(shell git tag | grep cs | sort | tail -n1)
deploy: $(bin)
	zip prepandoc-$(ver).zip $(bin) $(output1) LICENSE.txt

include Makefile.test

include Makefile.mono.options

# vi: ft=make:et:ts=4:fdm=marker
