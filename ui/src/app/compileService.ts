import { Injectable } from '@angular/core';

import { HttpClient } from '@angular/common/http';
import { HttpHeaders } from '@angular/common/http';

@Injectable()
export class CompileService {
     httpOptions = {
        headers: new HttpHeaders({
          'Content-Type':  'application/json',
          'Authorization': 'my-auth-token'
        })
      };
  constructor(private http: HttpClient) { }

//   approveByfinancialApprover(ticketId: string, message: string) {
//     const options = new TicketRestOptions();
//     options.resourcePath = 'api/settlements/' + ticketId + '/flow/financialapprove';
//     options.host = this.config.settlementAPIHostUrl;
//     return this.restService.post(options.getUrlForAction(null), {Data: message}, this.restService.getDefaultHeader());
//   }

  get()
  {
    return this.http.get("http://localhost:5000/api/compiler/");
  }

  postString(string: string)
  {
    return this.http.post("http://localhost:5000/api/compiler/", {Text: string}, this.httpOptions);
  }
}
