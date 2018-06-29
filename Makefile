path_ref := /c/Users/user1/Desktop/pandoc-template
FC := $(path_ref)/template/template_meiryo.docx
FB := $(path_ref)/config/crossref_config.yaml
FT := $(path_ref)/config/bibliography.bib

path_doc := .

python := python3  # /c/app/Python36/python.exe
pandoc_path := # /s/app/Pandoc/
pandoc := $(pandoc_path)pandoc  # .exe
# pandoc_crossref := $(pandoc_path)/pandoc-crossref.exe
opts_pandoc := \

#              -F pandoc-crossref \
#              -M crossrefYaml="$(FC)" \
#              --bibliography "$(FB)" \
#              --reference-doc="$(FT)" \

doc:
	# $(python) tools/prepandoc.py $(path_doc) temp.md
	# export PATH="$(PATH):$(pandoc_path)"; \
	$(python) tools/prepandoc.py $(path_doc) README.md temp.md
	$(pandoc) $(opts_pandoc) -o result.html temp.md
	# $(pandoc) $(opts_pandoc) -o abc.docx README.md temp.md


