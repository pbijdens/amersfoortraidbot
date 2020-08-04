import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { BotsDashboardComponent } from './bots-dashboard.component';

describe('BotsDashboardComponent', () => {
  let component: BotsDashboardComponent;
  let fixture: ComponentFixture<BotsDashboardComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ BotsDashboardComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(BotsDashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
