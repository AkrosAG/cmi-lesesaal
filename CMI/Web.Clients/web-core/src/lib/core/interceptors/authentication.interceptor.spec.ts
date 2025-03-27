import { HTTP_INTERCEPTORS, HttpClient, provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import {inject, TestBed} from '@angular/core/testing';
import {AuthenticationInterceptor} from './authentication.interceptor';
import {Session} from '../model/session';
import {ClientContext} from '../services/clientContext';
import {ClientModel} from '../services/clientModel';

describe('AuthenticationInterceptor', () => {
	let clientContext = new ClientContext(new ClientModel());

	beforeEach(() => {
		clientContext.currentSession = <Session>{ authenticated: true };

		TestBed.configureTestingModule({
    imports: [],
    providers: [
        { provide: HTTP_INTERCEPTORS, useClass: AuthenticationInterceptor, multi: true },
        { provide: ClientContext, useFactory: () => clientContext },
        provideHttpClient(withInterceptorsFromDi()),
        provideHttpClientTesting()
    ]
});
	});

	describe('intercept', () => {
		it('should set withCredentials if session is authenticated',
			inject([HttpClient, HttpTestingController], (httpClient: HttpClient, httpMock: HttpTestingController) => {
				const requestUrl = 'http://demo.ch/api/entities';

				httpClient
					.get(requestUrl)
					.subscribe();

				const req = httpMock.expectOne(requestUrl);

				expect(req.request.withCredentials).toBe(true);
			}));

		it('should skip authorization header without an auth token in session set',
			inject([HttpClient, HttpTestingController], (httpClient: HttpClient, httpMock: HttpTestingController) => {
				const requestUrl = 'http://demo.ch/api/entities';
				clientContext.currentSession.authenticated = false;

				httpClient
					.get(requestUrl)
					.subscribe();

				const req = httpMock.expectOne(requestUrl);

				expect(req.request.withCredentials).toBe(false);
			}));
	});
});
