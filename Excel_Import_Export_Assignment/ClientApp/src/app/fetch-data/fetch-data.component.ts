import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ResultModel } from '../ResultViewModel/ResultModel';
import { API_URL } from '../Common/Constants';

@Component({
  selector: 'app-fetch-data',
  templateUrl: './fetch-data.component.html'
})
export class FetchDataComponent implements OnInit {
  contracts: any = [];

  constructor(private http: HttpClient) { }

  ngOnInit(): void {
    this.fetchContracts();
  }

  fetchContracts(): void {
    this.http.get<Array<ContractBasicInfo>>(API_URL).subscribe(
      (data) => {
        debugger;
        this.contracts = data;
      },
      error => {
        console.log('Failed to fetch contracts:', error);
      }
    );
  }

  exportData(): void {
    debugger;
    this.http.post<ResultModel<boolean>>(API_URL + '/exportData',null)
      .subscribe(
        (response) => {        

          if (response.isSuccess) {
            alert(response.message);
          } else {
            alert(response.message);
          }
        }
      );
  }
}

export class ContractBasicInfo {
  id: string="";
  contractName: string="";
  clientName: string = "";
  startDate: Date = new Date;
  endDate: Date = new Date;
  laborCategories: Array<LaborCategory> = [];
}

export class LaborCategory {
  id: string = "";
  categoryName: string = "";
  ratePerHour: number =0;
  contractId: string = "";
}



