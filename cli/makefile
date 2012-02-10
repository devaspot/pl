# $Id: makefile,v 1.1.1.1 2004/06/03 08:59:35 master Exp $

!IFNDEF BIN_DIR
BIN_DIR=..\bin
!ENDIF

# Sources and Binaries

EXE=$(BIN_DIR)
LIB=$(EXE);
SRC=ios.cs cli.cs
REF=core.dll

# Target Files

TARGETS=$(EXE)\cli.exe
DBGINFO=$(EXE)\cli.pdb

# Rules

all: $(TARGETS)


$(EXE)\cli.exe: $(SRC)
	csc /debug+ /target:exe /r:$(REF) /lib:$(LIB) /out:$@ $(SRC)

clean:
	@for %i in ($(TARGETS)) do @if exist %i @erase %i
	@for %i in ($(DBGINFO)) do @if exist %i @erase %i
