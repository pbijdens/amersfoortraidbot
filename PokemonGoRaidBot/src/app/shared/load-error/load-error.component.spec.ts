import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { LoadErrorComponent } from './load-error.component';

describe('LoadErrorComponent', () => {
  let component: LoadErrorComponent;
  let fixture: ComponentFixture<LoadErrorComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ LoadErrorComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(LoadErrorComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
