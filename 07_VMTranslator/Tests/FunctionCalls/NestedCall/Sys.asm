// function Sys.init 0
(Sys.init)
// push constant 4000	
@4000
D=A
@SP    // Push D
A=M
M=D
@SP
M=M+1
// pop pointer 0
@SP    // Pop To D
M=M-1
@SP
A=M
D=M
@THIS
M=D
// push constant 5000
@5000
D=A
@SP    // Push D
A=M
M=D
@SP
M=M+1
// pop pointer 1
@SP    // Pop To D
M=M-1
@SP
A=M
D=M
@THAT
M=D
// call Sys.main 0
@Sys.main$ret.0
D=A
@SP    // Push D
A=M
M=D
@SP
M=M+1
@LCL
D=M
@SP    // Push D
A=M
M=D
@SP
M=M+1
@ARG
D=M
@SP    // Push D
A=M
M=D
@SP
M=M+1
@THIS
D=M
@SP    // Push D
A=M
M=D
@SP
M=M+1
@THAT
D=M
@SP    // Push D
A=M
M=D
@SP
M=M+1
@SP
D=M
@5
D=D-A
@0
D=D-A
@ARG
M=D
@SP
D=M
@LCL
M=D
@Sys.main
0; JMP
(Sys.main$ret.0)
// pop temp 1
@5
D=A
@1
D=D+A
@Sys.temp
M=D
@SP    // Pop To D
M=M-1
@SP
A=M
D=M
@Sys.temp
A=M
M=D
// label LOOP
(LOOP)
// goto LOOP
@LOOP
0; JMP
// function Sys.main 5
(Sys.main)
@5
D=A
@SP
M=M+D
// push constant 4001
@4001
D=A
@SP    // Push D
A=M
M=D
@SP
M=M+1
// pop pointer 0
@SP    // Pop To D
M=M-1
@SP
A=M
D=M
@THIS
M=D
// push constant 5001
@5001
D=A
@SP    // Push D
A=M
M=D
@SP
M=M+1
// pop pointer 1
@SP    // Pop To D
M=M-1
@SP
A=M
D=M
@THAT
M=D
// push constant 200
@200
D=A
@SP    // Push D
A=M
M=D
@SP
M=M+1
// pop local 1
@LCL
D=M
@1
D=D+A
@Sys.temp
M=D
@SP    // Pop To D
M=M-1
@SP
A=M
D=M
@Sys.temp
A=M
M=D
// push constant 40
@40
D=A
@SP    // Push D
A=M
M=D
@SP
M=M+1
// pop local 2
@LCL
D=M
@2
D=D+A
@Sys.temp
M=D
@SP    // Pop To D
M=M-1
@SP
A=M
D=M
@Sys.temp
A=M
M=D
// push constant 6
@6
D=A
@SP    // Push D
A=M
M=D
@SP
M=M+1
// pop local 3
@LCL
D=M
@3
D=D+A
@Sys.temp
M=D
@SP    // Pop To D
M=M-1
@SP
A=M
D=M
@Sys.temp
A=M
M=D
// push constant 123
@123
D=A
@SP    // Push D
A=M
M=D
@SP
M=M+1
// call Sys.add12 1
@Sys.add12$ret.0
D=A
@SP    // Push D
A=M
M=D
@SP
M=M+1
@LCL
D=M
@SP    // Push D
A=M
M=D
@SP
M=M+1
@ARG
D=M
@SP    // Push D
A=M
M=D
@SP
M=M+1
@THIS
D=M
@SP    // Push D
A=M
M=D
@SP
M=M+1
@THAT
D=M
@SP    // Push D
A=M
M=D
@SP
M=M+1
@SP
D=M
@5
D=D-A
@1
D=D-A
@ARG
M=D
@SP
D=M
@LCL
M=D
@Sys.add12
0; JMP
(Sys.add12$ret.0)
// pop temp 0
@5
D=A
@0
D=D+A
@Sys.temp
M=D
@SP    // Pop To D
M=M-1
@SP
A=M
D=M
@Sys.temp
A=M
M=D
// push local 0
@LCL
D=M
@0
D=D+A
A=D
D=M
@SP    // Push D
A=M
M=D
@SP
M=M+1
// push local 1
@LCL
D=M
@1
D=D+A
A=D
D=M
@SP    // Push D
A=M
M=D
@SP
M=M+1
// push local 2
@LCL
D=M
@2
D=D+A
A=D
D=M
@SP    // Push D
A=M
M=D
@SP
M=M+1
// push local 3
@LCL
D=M
@3
D=D+A
A=D
D=M
@SP    // Push D
A=M
M=D
@SP
M=M+1
// push local 4
@LCL
D=M
@4
D=D+A
A=D
D=M
@SP    // Push D
A=M
M=D
@SP
M=M+1
// add
@SP    // Pop To D
M=M-1
@SP
A=M
D=M
@SP
AM=M-1
D=M+D
@SP    // Push D
A=M
M=D
@SP
M=M+1
// add
@SP    // Pop To D
M=M-1
@SP
A=M
D=M
@SP
AM=M-1
D=M+D
@SP    // Push D
A=M
M=D
@SP
M=M+1
// add
@SP    // Pop To D
M=M-1
@SP
A=M
D=M
@SP
AM=M-1
D=M+D
@SP    // Push D
A=M
M=D
@SP
M=M+1
// add
@SP    // Pop To D
M=M-1
@SP
A=M
D=M
@SP
AM=M-1
D=M+D
@SP    // Push D
A=M
M=D
@SP
M=M+1
// return
@LCL
D=M
@endFrameTemp
M=D
@endFrameTemp
D=M
@5
A=D-A
D=M
@retAddrTemp
M=D
@SP    // Pop To D
M=M-1
@SP
A=M
D=M
@ARG
A=M
M=D
@ARG
D=M+1
@SP
M=D
@endFrameTemp
D=M
@1
A=D-A
D=M
@THAT
M=D
@endFrameTemp
D=M
@2
A=D-A
D=M
@THIS
M=D
@endFrameTemp
D=M
@3
A=D-A
D=M
@ARG
M=D
@endFrameTemp
D=M
@4
A=D-A
D=M
@LCL
M=D
@retAddrTemp
A=M
0; JMP
// function Sys.add12 0
(Sys.add12)
// push constant 4002
@4002
D=A
@SP    // Push D
A=M
M=D
@SP
M=M+1
// pop pointer 0
@SP    // Pop To D
M=M-1
@SP
A=M
D=M
@THIS
M=D
// push constant 5002
@5002
D=A
@SP    // Push D
A=M
M=D
@SP
M=M+1
// pop pointer 1
@SP    // Pop To D
M=M-1
@SP
A=M
D=M
@THAT
M=D
// push argument 0
@ARG
D=M
@0
D=D+A
A=D
D=M
@SP    // Push D
A=M
M=D
@SP
M=M+1
// push constant 12
@12
D=A
@SP    // Push D
A=M
M=D
@SP
M=M+1
// add
@SP    // Pop To D
M=M-1
@SP
A=M
D=M
@SP
AM=M-1
D=M+D
@SP    // Push D
A=M
M=D
@SP
M=M+1
// return
@LCL
D=M
@endFrameTemp
M=D
@endFrameTemp
D=M
@5
A=D-A
D=M
@retAddrTemp
M=D
@SP    // Pop To D
M=M-1
@SP
A=M
D=M
@ARG
A=M
M=D
@ARG
D=M+1
@SP
M=D
@endFrameTemp
D=M
@1
A=D-A
D=M
@THAT
M=D
@endFrameTemp
D=M
@2
A=D-A
D=M
@THIS
M=D
@endFrameTemp
D=M
@3
A=D-A
D=M
@ARG
M=D
@endFrameTemp
D=M
@4
A=D-A
D=M
@LCL
M=D
@retAddrTemp
A=M
0; JMP
