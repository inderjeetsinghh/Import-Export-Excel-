export class ResultModel<T> {
  result: T;
  isSuccess: boolean=false;
  message: string ="";
  errors: string[] = [];

  constructor(result: T) {
    this.result = result;
  }
}
