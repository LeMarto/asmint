# asmint
8bit "Assembler" Interpreter in C# Net Core

# Assumptions
-*Instruction Pointer* always points to the next instruction to be executed
-*Stack Pointer* points to the current element on the top of the stack
-Code gets loaded at the top of the memory
-Stack is at the bottom of the memory
-MOV operation uses the *Base Segment* to know what segment to use