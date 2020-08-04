import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { RaidsDashboardComponent } from './raids-dashboard.component';

describe('RaidsDashboardComponent', () => {
  let component: RaidsDashboardComponent;
    let fixture: ComponentFixture<RaidsDashboardComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
        declarations: [ RaidsDashboardComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
      fixture = TestBed.createComponent(RaidsDashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
