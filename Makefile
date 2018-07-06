platform := $(shell uname)
ifeq ($(platform),Linux)
    prepandoc := tools/prepandoc.exe
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

CS_OPTIONS := -r:System.Windows.Forms.dll -r:System.Drawing.dll \
              -debug
# CS_OPTIONS += -r:System.Collections.Generic.dll

bin := prepandoc.exe
src := prepandoc.cs config.cs common.cs

debug: $(bin)
	mono --debug $(bin) $(path_doc) $(path_doc)/README.md

build: $(bin)

$(bin): $(src)
	mcs $(CS_OPTIONS) -out:$@ $^


FC := $(pandoc_tmps)/template/template_meiryo.docx
FB := $(pandoc_tmps)/config/crossref_config.yaml
FT := $(pandoc_tmps)/config/bibliography.bib

path_doc := .

opts_pandoc := \
               --toc --template=tools/template1.html
#              -F pandoc-crossref \
#              -M crossrefYaml="$(FC)" \
#              --bibliography "$(FB)" \
#              --reference-doc="$(FT)" \

doc:
	# export PATH="$(PATH):$(pandoc_path)";
	$(prepandoc) $(path_doc) $(path_doc)/README.md temp.md
	$(pandoc) $(opts_pandoc) -o $(output) temp.md
	$(launch) $(output)


