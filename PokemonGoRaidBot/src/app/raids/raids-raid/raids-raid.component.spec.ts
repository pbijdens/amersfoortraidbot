import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { RaidsRaidComponent } from './raids-raid.component';

describe('RaidsRaidComponent', () => {
  let component: RaidsRaidComponent;
  let fixture: ComponentFixture<RaidsRaidComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ RaidsRaidComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(RaidsRaidComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
