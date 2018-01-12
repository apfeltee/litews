
csfiles_orig = $(wildcard ./lib/*.cs) main.cs
csfiles = $(subst /,\\,${csfiles_orig})

all:
	csc -nologo -debug+ -warn:4 -out:run.exe $(csfiles)

