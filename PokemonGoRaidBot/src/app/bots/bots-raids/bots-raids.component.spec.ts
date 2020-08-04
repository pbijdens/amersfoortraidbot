import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { BotsRaidsComponent } from './bots-raids.component';

describe('BotsRaidsComponent', () => {
  let component: BotsRaidsComponent;
  let fixture: ComponentFixture<BotsRaidsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ BotsRaidsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(BotsRaidsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
