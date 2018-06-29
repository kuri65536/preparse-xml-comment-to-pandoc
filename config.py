from typing import Text


f_output_empty_block = False


def format_block_name(name):
    # type: (Text) -> Text
    return "### " + name + "\n"


def format_file_name(name):
    # type: (Text) -> Text
    return "<!-- " + name + " -->\n"


Text
# vi: ft=python
