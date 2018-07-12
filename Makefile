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

all: doc

include Makefile.mono.options


CS_OPTIONS := -r:System.Windows.Forms.dll -r:System.Drawing.dll \
              -r:$(bin_opt) \
              -debug
# CS_OPTIONS += -r:System.Collections.Generic.dll

bin := prepandoc.exe
src := prepandoc.cs config.cs common.cs versions.cs


build: $(bin)

$(bin): $(src) tag
	mcs $(CS_OPTIONS) -out:$@ $(filter-out tag,$^)


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

doc: $(bin)
	# export PATH="$(PATH):$(pandoc_path)";
	$(prepandoc) $(path_doc) $(path_doc)/source.md temp.md
	$(pandoc) $(opts_pandoc1) -o $(output1) temp.md
	$(pandoc) $(opts_pandoc) -o $(output) $(output1)
	$(launch) $(output)

ref: $(bin)
	doxygen Doxyfile

ifeq (x$(tag),)
tag:
	echo not specified
else
tag:
	git tag $(tag) $(tag_force)
	sed -i.bak1 's/ver = ".*"/ver = "$(tag)"/' versions.cs
	ref=$$(git tag --sort=creatordate \
	               --format='%(objectname:short)' | head -n1); \
	echo $$ref; \
	sed -i.bak2 "s/rev = \".*\"/rev = \"$$ref\"/" versions.cs
endif

deploy: ver:=$(shell git tag | grep cs | sort | tail -n1)
deploy: $(bin) $(bin_opt)
	zip prepandoc-$(ver).zip $^ $(output1) LICENSE.txt

include Makefile.test


# vi: ft=make:et:ts=4:fdm=marker
