import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { map} from 'rxjs';

@Component({
  selector: 'app-report-component',
  templateUrl: './report.component.html'
})
export class ReportComponent implements OnInit {

  base64: string = "";

  constructor(private httpClient: HttpClient) { }

  ngOnInit(): void {
    this.httpClient
      .get('http://localhost:57915/api/values/pdf')
      .pipe(
        map((response) => {
          this.base64 = response as string;
        })
      )
      .subscribe();
  }
}
