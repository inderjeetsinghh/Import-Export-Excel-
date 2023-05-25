import { Component, ElementRef, ViewChild } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { ResultModel } from '../ResultViewModel/ResultModel';
import { API_URL } from '../Common/Constants';



@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
})
export class HomeComponent {
  file: File | null = null;

  constructor(private http: HttpClient, private router: Router) { }

  onFileChange(evt: any) {
    this.file = evt.target.files[0];
    this.uploadFile();
  }

  uploadFile(): void {
    if (this.file) {
      const formData: FormData = new FormData();
      formData.append('file', this.file, this.file.name);

      this.http.post<ResultModel<boolean>>(API_URL, formData)
        .subscribe(
          (response) => {
            if (response.isSuccess) {
              alert('File uploaded successfully, Data saved in DB');
              this.router.navigate(['/fetch-data']);
            } else {
              alert(response.message);
            }

          }
        );
    }
   
  }

}
