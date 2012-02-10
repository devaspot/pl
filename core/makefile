# $Id: makefile,v 1.1 2004/06/03 09:02:42 master Exp $

!IFNDEF BIN_DIR
BIN_DIR=..\bin
!ENDIF

# Sources and Binaries

EXE=$(BIN_DIR)
LIB=$(EXE);lib
SRC=ios.cs lex.cs syn.cs var.cs blk.cs emt.cs vma.cs asm.cs exe.cs num.cs lib.cs

# Target Files

TARGETS=$(EXE)\core.dll
DBGINFO=$(EXE)\core.pdb

# Rules

all: $(TARGETS)


$(EXE)\core.dll: $(SRC)
	csc /debug+ /target:library /out:$@ $(SRC)

clean:
	@for %i in ($(TARGETS)) do @if exist %i @erase %i
	@for %i in ($(DBGINFO)) do @if exist %i @erase %i
