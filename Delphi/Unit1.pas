{$define OPERATOR_CONCAT}

unit Unit1;

interface

uses
  Winapi.Windows, Winapi.Messages, System.SysUtils, System.Variants, System.Classes, Vcl.Graphics,
  Vcl.Controls, Vcl.Forms, Vcl.Dialogs, Vcl.StdCtrls;

type
  TForm1 = class(TForm)
    Button1: TButton;
    LB: TListBox;
    procedure Button1Click(Sender: TObject);
  private
    { Private declarations }
    function Test1(Cycles,Count:Integer;Lengths:TArray<Integer>):Integer;
    procedure PerformTest1;
  public
    { Public declarations }
  end;

var
  Form1: TForm1;

implementation

{$R *.dfm}

procedure TForm1.Button1Click(Sender: TObject);
begin
PerformTest1;
end;

procedure TForm1.PerformTest1;
var BaseLengths,Lengths,Coeff:TArray<Integer>;
    i,j,Cycles,Count:Integer;

  procedure WriteInfo(PerfRes:Integer);
  var S:String;
      i:Integer;
  begin
  S:='Начало теста Repeat='+IntToStr(Cycles)+' Count='+
    IntToStr(Count)+' Длины строк: ';
  for i in Lengths do S:=S+' '+IntToStr(i);
  S:=S+' Результат: '+IntToStr(PerfRes)+' мс';
  LB.Items.Add(S);
  Application.ProcessMessages;
  end;

  procedure PerformPack;
  var i,j:Integer;
  begin
  for i in BaseLengths do
    begin
    SetLength(Lengths,Length(Coeff));
    //Lengths:=Coeff;
    for j:=0 to High(Lengths) do Lengths[j]:=Coeff[j];
    for j:=0 to High(Lengths) do
      Lengths[j]:=Lengths[j]*i;
    WriteInfo(Test1(Cycles,Count,Lengths));
    end;
  end;

begin
Cycles:=100000000;
Count:=1;
for i:=1 to 4 do
  begin
  BaseLengths:=TArray<Integer>.Create(1, 2, 5, 10, 20, 50, 100);
  Coeff:=TArray<Integer>.Create(1);
  PerformPack;
  BaseLengths:=TArray<Integer>.Create(1, 2, 5, 10, 20, 50);
  Coeff:=TArray<Integer>.Create(1,2);
  PerformPack;
  BaseLengths:=TArray<Integer>.Create(1, 2, 5, 10);
  Coeff:=TArray<Integer>.Create(1,10,2,5);
  PerformPack;
  Cycles:= Cycles div 100;
  Count:=Count*100;
  end;
LB.Items.SaveToFile('Result.txt');
end;

function TForm1.Test1(Cycles,Count:Integer;Lengths:TArray<Integer>): Integer;
var S:String;
{$ifdef OPERATOR_CONCAT}
    B:String;
{$else}
    B:TStringBuilder;
{$endif}
    i,j:Integer;
    Strings:TArray<String>;
begin
Result:=GetTickCount;
SetLength(Strings,Length(Lengths));
for i:=0 to High(Strings) do Strings[i]:=StringOfChar('a',Lengths[i]);
for i:=1 to Cycles do
  begin
{$ifdef OPERATOR_CONCAT}
  B:='';
  for j:=1 to Count do
    for S in Strings do B:=B+S;
{$else}
  B:=TStringBuilder.Create;
  for j:=1 to Count do
    for S in Strings do B.Append(S);
  B.ToString;
  B.Free;
{$endif}
  end;
Result:=GetTickCount-Result;
end;

end.
